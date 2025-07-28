using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.CartDTO;
using PharmaLink_API.Repository.Interfaces;
//using PharmaLink_API.Utility;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
//using Stripe.BillingPortal;

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IPharmacyStockRepository _pharmacyStockRepository;
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly StripeModel _StripeModel;
        public OrdersController(ICartRepository cartRepository, IPatientRepository patientRepository, IOrderHeaderRepository orderHeaderRepository, IOrderDetailRepository orderDetailRepository, IOptions<StripeModel> stripeOptions, IPharmacyStockRepository pharmacyStockRepository, IPharmacyRepository pharmacyRepository)
        {
            _cartRepository = cartRepository;
            _patientRepository = patientRepository;
            _orderHeaderRepository = orderHeaderRepository;
            _orderDetailRepository = orderDetailRepository;
            _StripeModel = stripeOptions.Value;
            _pharmacyStockRepository = pharmacyStockRepository;
            _pharmacyRepository = pharmacyRepository;
        }

        [Authorize(Roles = "Patient")]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitOrder([FromBody] SubmitOrderRequestDTO dto)
        {
            // 1. Get accountId from JWT
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");

            // 2. Get Patient and Account
            var user = await _patientRepository.GetAsync(
                u => u.AccountId == accountId, true, 
                x => x.Account,
                x => x.CartItems
            );

            if (user == null || user.Account == null || user.CartItems == null || !user.CartItems.Any())
                return BadRequest("No items in cart.");

            int patientId = user.PatientId;
            var cartItems = user.CartItems.ToList();
            decimal totalPrice = 0;
            int pharmacyId = cartItems.First().PharmacyId;

            // 3. Validate Cart Items and Calculate total
            foreach (var item in cartItems)
            {
                var stock = await _pharmacyStockRepository.GetAsync(
                    s => s.DrugId == item.DrugId && s.PharmacyId == item.PharmacyId
                );

                if (stock == null)
                    return BadRequest($"Drug ID {item.DrugId} not found in pharmacy stock.");

                if (item.Quantity > stock.QuantityAvailable)
                    return BadRequest($"Not enough stock for Drug ID {item.DrugId}.");

                totalPrice += stock.Price * item.Quantity;
            }

            // 4. Create Order
            var order = new PharmaLink_API.Models.Order
            {
                PatientId = patientId,
                Name = user.Name,
                PhoneNumber = user.Account.PhoneNumber,
                Email = user.Account.Email,
                Country = user.Country,
                Address = user.Address,
                TotalPrice = totalPrice,
                OrderDate = DateTime.UtcNow,
                PharmacyId = pharmacyId,
                PaymentStatus = SD.PaymentStatusPending,
                Status = SD.StatusPending,
                PaymentMethod = dto.PaymentMethod
            };

            await _orderHeaderRepository.CreateAsync(order);
            await _orderHeaderRepository.SaveAsync();

            // 5. Create OrderDetails
            foreach (var item in cartItems)
            {
                var stock = await _pharmacyStockRepository.GetAsync(
                    s => s.DrugId == item.DrugId && s.PharmacyId == item.PharmacyId
                );

                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderID,
                    DrugId = item.DrugId,
                    PharmacyId = item.PharmacyId,
                    Quantity = item.Quantity,
                    Price = stock.Price
                };

                await _orderDetailRepository.CreateAsync(orderDetail);
            }

            await _orderDetailRepository.SaveAsync();

            // 6. Update PharmacyStock
            foreach (var item in cartItems)
            {
                var stock = await _pharmacyStockRepository.GetAsync(
                    s => s.DrugId == item.DrugId && s.PharmacyId == item.PharmacyId
                );

                if (stock != null)
                {
                    stock.QuantityAvailable -= item.Quantity;
                }
            }

            await _pharmacyStockRepository.SaveAsync();

            // 7. Clear the Cart
            await _cartRepository.RemoveRangeAsync(user.CartItems.ToList());
            await _cartRepository.SaveAsync();

            return Ok(new
            {
                OrderId = order.OrderID,
                PaymentMethod = dto.PaymentMethod,
                Message = "Order submitted successfully from cart."
            });
        }

        [HttpPost("CreateCheckoutSession")]
        public async Task<ActionResult> CreateCheckoutSession([FromBody]int orderId)
        {
            StripeConfiguration.ApiKey = _StripeModel.SecretKey;
            var order = await _orderHeaderRepository.GetAsync(o => o.OrderID == orderId, tracking:true, x => x.OrderDetails);
            if (order == null)
            {
                return NotFound("Order not found.");
            }

            var orderDetails = await _orderDetailRepository.GetAllAsync(od => od.OrderId == orderId, x => x.PharmacyProduct.Drug);


            var domain = "http://localhost:4200/client";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{domain}/success?orderId={order.OrderID}",
                CancelUrl = $"{domain}/cancel"
            };

            foreach (var item in orderDetails)
            {
                if (item.PharmacyProduct?.Drug == null)
                    return BadRequest("Drug information missing for some cart items.");

                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.PharmacyProduct.Drug.CommonName ?? "Unnamed Drug",
                            Description = item.PharmacyProduct.Drug.Description ?? "No description",
                        },
                        UnitAmount = (long)(item.Price * 100),
                    },
                    Quantity = item.Quantity,
                });
            }

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            _orderHeaderRepository.UpdateStripePaymentID(orderId, session.Id, session.PaymentIntentId);
            await _orderHeaderRepository.SaveAsync();

            return Ok(new { Id = session.Id, Url = session.Url });
        }

        [HttpPost("stripe-webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            try
            {
                Request.EnableBuffering();
                var json = await new StreamReader(Request.Body).ReadToEndAsync();
                Request.Body.Position = 0;

                var signatureHeader = Request.Headers["Stripe-Signature"];
                Console.WriteLine("Stripe-Signature: " + signatureHeader);

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signatureHeader,
                    _StripeModel.WebhookSecret,
                    throwOnApiVersionMismatch: false
                );

                Console.WriteLine("Stripe Event Type: " + stripeEvent.Type);

                if (stripeEvent.Type == "checkout.session.completed") 
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session == null)
                    {
                        Console.WriteLine("Session is null");
                        return BadRequest("Invalid session object.");
                    }

                    Console.WriteLine("Stripe Session ID: " + session.Id);

                    StripeConfiguration.ApiKey = _StripeModel.SecretKey;
                    var service = new SessionService();
                    var fullSession = await service.GetAsync(session.Id);

                    var order = await _orderHeaderRepository.GetAsync(o => o.SessionId == session.Id);
                    if (order != null)
                    {
                        //order.Status = SD.StatusApproved;
                        order.PaymentStatus = SD.PaymentStatusApproved;
                        order.PaymentIntentId = fullSession.PaymentIntentId;
                        await _orderHeaderRepository.SaveAsync();
                        Console.WriteLine($"Order updated: OrderID = {order.OrderID}");
                    }
                    else
                    {
                        Console.WriteLine($"No order found for session ID: {session.Id}");
                    }
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                Console.WriteLine("StripeException occurred:");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"HttpStatusCode: {ex.HttpStatusCode}");
                Console.WriteLine($"StripeError.Type: {ex.StripeError?.Type}");
                Console.WriteLine($"StripeError.Message: {ex.StripeError?.Message}");
                Console.WriteLine($"StripeError.Code: {ex.StripeError?.Code}");
                return BadRequest($"Stripe error: {ex.Message}");
            }

            catch (Exception ex)
            {
                Console.WriteLine("General Exception: " + ex.Message);
                return BadRequest("Webhook processing error.");
            }
        }

        [Authorize(Roles = "user")]
        [HttpPost("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");

            var order = await _orderHeaderRepository.GetAsync(
                o => o.OrderID == orderId, tracking: true,
                x => x.OrderDetails
            );

            if (order == null)
                return NotFound("Order not found.");

            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null || patient.PatientId != order.PatientId)
                return Forbid("Not authorized to cancel this order.");

            if (order.Status == SD.StatusCancelled)
                return BadRequest("Order is already cancelled.");

            if (order.Status == SD.StatusApproved)
            {
                // Stripe refund if paid by card
                if (order.PaymentMethod != "Cash" && order.PaymentStatus == SD.PaymentStatusApproved)
                {
                    StripeConfiguration.ApiKey = _StripeModel.SecretKey;

                    var refundOptions = new RefundCreateOptions
                    {
                        PaymentIntent = order.PaymentIntentId,
                        Reason = RefundReasons.RequestedByCustomer,
                    };

                    var refundService = new RefundService();
                    Refund refund = await refundService.CreateAsync(refundOptions);

                    order.PaymentStatus = SD.PaymentStatusRefunded;
                }
                else
                {
                    return BadRequest("Cannot cancel an already approved order paid in cash.");
                }
            }
            else
            {
                order.PaymentStatus = SD.PaymentStatusRefunded;
            }

            // Cancel and restock
            order.Status = SD.StatusCancelled;

            foreach (var item in order.OrderDetails)
            {
                var stock = await _pharmacyStockRepository.GetAsync(
                    s => s.DrugId == item.DrugId && s.PharmacyId == item.PharmacyId
                );
                if (stock != null)
                {
                    stock.QuantityAvailable += item.Quantity;
                }
            }

            await _orderHeaderRepository.SaveAsync();
            await _pharmacyStockRepository.SaveAsync();

            return Ok(new
            {
                Message = $"Order #{orderId} has been cancelled and refund issued if applicable."
            });
        }



        //******************Pharmacy Only Endpoints******************//

        [Authorize(Roles = "Pharmacy")]
        [HttpPost("accept/{orderId}")]
        public async Task<IActionResult> AcceptOrder(int orderId)
        {
            var order = await _orderHeaderRepository.GetAsync(o => o.OrderID == orderId, true, x => x.OrderDetails);
            if (order == null)
                return NotFound("Order not found.");

            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Forbid("AccountId missing.");

            var pharmacy = await _pharmacyRepository.GetAsync(p => p.AccountId == accountId);
            if (pharmacy == null)
                return Forbid("Pharmacy not found.");

            if (order.PharmacyId!= pharmacy.PharmacyID)
                return Forbid("You are not authorized to update this order.");

            if (order.Status != SD.StatusPending)
                return BadRequest("Only pending orders can be accepted.");

            order.Status = SD.StatusApproved;
            await _orderHeaderRepository.SaveAsync();

            return Ok(new { Message = $"Order #{orderId} has been accepted." });
        }

        [Authorize(Roles = "Pharmacy")]
        [HttpPost("reject/{orderId}")]
        public async Task<IActionResult> RejectOrder(int orderId)
        {
            var order = await _orderHeaderRepository.GetAsync(o => o.OrderID == orderId, true, x => x.OrderDetails);
            if (order == null)
                return NotFound("Order not found.");

            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Forbid("AccountId missing.");

            var pharmacy = await _pharmacyRepository.GetAsync(p => p.AccountId == accountId);
            if (pharmacy == null)
                return Forbid("Pharmacy not found.");

            if (order.PharmacyId != pharmacy.PharmacyID)
                return Forbid("You are not authorized to update this order.");

            if (order.Status != SD.StatusPending)
                return BadRequest("Only pending orders can be rejected.");

            order.Status = SD.StatusRejected;

            if (order.PaymentStatus == SD.PaymentStatusApproved)
            {
                // Stripe refund if paid by card
                if (order.PaymentMethod != "Cash")
                {
                    StripeConfiguration.ApiKey = _StripeModel.SecretKey;
                    var refundOptions = new RefundCreateOptions
                    {
                        PaymentIntent = order.PaymentIntentId,
                        Reason = RefundReasons.RequestedByCustomer,
                    };
                    var refundService = new RefundService();
                    Refund refund = await refundService.CreateAsync(refundOptions);
                    order.PaymentStatus = SD.PaymentStatusRefunded;
                }
            }

            foreach (var item in order.OrderDetails)
            {
                var stock = await _pharmacyStockRepository.GetAsync(s => s.PharmacyId == item.PharmacyId && s.DrugId == item.DrugId);
                if (stock != null)
                {
                    stock.QuantityAvailable += item.Quantity;
                }
            }

            await _orderHeaderRepository.SaveAsync();
            await _pharmacyStockRepository.SaveAsync();

            return Ok(new { Message = $"Order #{orderId} has been rejected." });
        }

        [Authorize(Roles = "Pharmacy")]
        [HttpGet("orders")]
        public async Task<IActionResult> GetPharmacyOrders()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Forbid("AccountId missing.");

            var pharmacy = await _pharmacyRepository.GetAsync(p => p.AccountId == accountId);
            if (pharmacy == null)
                return Forbid("Pharmacy not found.");

            var orders = await _orderHeaderRepository.GetAllAsync( o => o.PharmacyId == pharmacy.PharmacyID,
                x => x.OrderDetails,
                x => x.Patient,
                x => x.Pharmacy);

            if (orders == null || !orders.Any())
                return NotFound("No orders found for this pharmacy.");

            var result = orders.Select(o => new
            {
                o.OrderID,
                o.OrderDate,
                o.Status,
                o.TotalPrice,
                o.PaymentStatus,
                OrderDetails = o.OrderDetails.Select(d => new {
                    d.DrugId,
                    d.Quantity
                })
            });

            return Ok(result);
        }

    }
}
