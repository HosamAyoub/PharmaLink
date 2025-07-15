using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PharmaLink_API.Models;
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
        private readonly IUserRepository _userRepository;
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly StripeModel _StripeModel;
        public OrdersController(ICartRepository cartRepository, IUserRepository userRepository, IOrderHeaderRepository orderHeaderRepository, IOrderDetailRepository orderDetailRepository, IOptions<StripeModel> stripeOptions)
        {
            _cartRepository = cartRepository;
            _userRepository = userRepository;
            _orderHeaderRepository = orderHeaderRepository;
            _orderDetailRepository = orderDetailRepository;
            _StripeModel = stripeOptions.Value;
        }

        [HttpPost]
        public async Task<ActionResult> SubmitOrder(int userId)
        {
            var cartItems = await _cartRepository.GetAllAsync(u => u.UserId == userId, x => x.PharmacyStocks);
            var user = await _userRepository.GetAsync(u => u.UserID == userId, true, x=>x.Account);

            if (user == null || !cartItems.Any() || user.Account == null)
            {
                return BadRequest("Invalid user or empty cart.");
            }

            var pharmacyId = cartItems.FirstOrDefault().PharmacyId;    

            var order = new PharmaLink_API.Models.Order
            {
                UserId = userId,
                Name = user.Name,
                PhoneNumber = user.MobileNumber,
                Email = user.Account.Email,
                Country = user.Country,
                Address = user.Address,
                TotalPrice = cartItems.Sum(c => c.Price * c.Quantity),
                OrderDate = DateTime.UtcNow,
                PharmacyId = pharmacyId,

                PaymentStatus = SD.PaymentStatusPending,
                Status = SD.StatusPending,
                PaymentMethod = "Cash"
            };

            await _orderHeaderRepository.CreateAsync(order);
            await _orderHeaderRepository.SaveAsync();

            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderID,
                    DrugId = item.DrugId,
                    PharmacyId = item.PharmacyId,
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                await _orderDetailRepository.CreateAsync(orderDetail);
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


            var domain = "http://localhost:4200";
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
                        UnitAmount = (long)(item.Price * 100), // Amount in cents
                    },
                    Quantity = item.Quantity,
                });
            }

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            // Save sessionId if needed for tracking
            _orderHeaderRepository.UpdateStripePaymentID(orderId, session.Id, session.PaymentIntentId);
            await _orderHeaderRepository.SaveAsync();

            return Ok(new { Id = session.Id, Url = session.Url });
        }

        [HttpPost("stripe-webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            try
            {
                Request.EnableBuffering(); // important to allow multiple reads
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
