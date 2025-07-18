using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.CartDTO;
using PharmaLink_API.Repository.IRepository;
using PharmaLink_API.Utility;
using Stripe;
using Stripe.Checkout;
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
        private readonly StripeModel _StripeModel;
        public OrdersController(ICartRepository cartRepository, IPatientRepository patientRepository, IOrderHeaderRepository orderHeaderRepository, IOrderDetailRepository orderDetailRepository, IOptions<StripeModel> stripeOptions, IPharmacyStockRepository pharmacyStockRepository)
        {
            _cartRepository = cartRepository;
            _patientRepository = patientRepository;
            _orderHeaderRepository = orderHeaderRepository;
            _orderDetailRepository = orderDetailRepository;
            _StripeModel = stripeOptions.Value;
            _pharmacyStockRepository = pharmacyStockRepository;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitOrder([FromBody] SubmitOrderRequestDTO request)
        {
            var patientId = request.PatientId;
            var items = request.Items;

            var cartItems = items; 
            var user = await _patientRepository.GetAsync(u => u.PatientId == patientId, true, x => x.Account);

            if (user == null || !items.Any() || user.Account == null)
            {
                return BadRequest("Invalid user or empty cart.");
            }

            decimal totalPrice = 0;
            int pharmacyId = items.First().PharmacyId;

            foreach (var item in items)
            {
                var stock = await _pharmacyStockRepository.GetAsync(
                    s => s.DrugId == item.DrugId && s.PharmacyId == item.PharmacyId
                );

                if (stock == null)
                    return BadRequest($"Drug with ID {item.DrugId} not found in stock.");

                if (item.Quantity > stock.QuantityAvailable)
                    return BadRequest($"Not enough stock for drug ID {item.DrugId}.");

                totalPrice += stock.Price * item.Quantity;
            }

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
                PaymentMethod = "Cash"
            };

            await _orderHeaderRepository.CreateAsync(order);
            await _orderHeaderRepository.SaveAsync();

            foreach (var item in items)
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

                stock.QuantityAvailable -= item.Quantity;
            }

            await _orderDetailRepository.SaveAsync();

            return Ok(new { OrderId = order.OrderID, Message = "Order submitted successfully." });
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

            var orderDetails = await _orderDetailRepository.GetAllAsync(od => od.OrderId == orderId, x => x.PharmacyStock.Drug);


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
                if (item.PharmacyStock?.Drug == null)
                    return BadRequest("Drug information missing for some cart items.");

                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.PharmacyStock.Drug.CommonName ?? "Unnamed Drug",
                            Description = item.PharmacyStock.Drug.CommonName ?? "No description",
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

                    var order = await _orderHeaderRepository.GetAsync(o => o.SessionId == session.Id);
                    if (order != null)
                    {
                        order.Status = SD.StatusApproved;
                        order.PaymentStatus = SD.PaymentStatusApproved;
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
    }
}
