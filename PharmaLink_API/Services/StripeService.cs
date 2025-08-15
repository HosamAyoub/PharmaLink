using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Core.Results;
using PharmaLink_API.Data;
using PharmaLink_API.Hubs;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.OrderDTO;
using PharmaLink_API.Repository;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using Order = PharmaLink_API.Models.Order;


namespace PharmaLink_API.Services
{
    public class StripeService : IStripeService
    {
        private readonly StripeModel _stripeModel;
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrderHub> _orderHubContext;


        /// <summary>
        /// Initializes a new instance of the <see cref="StripeService"/> class.
        /// </summary>
        /// <param name="stripeOptions">Stripe configuration options.</param>
        /// <param name="orderHeaderRepository">Order header repository.</param>
        /// <param name="orderDetailRepository">Order detail repository.</param>
        public StripeService(IOptions<StripeModel> stripeOptions, IOrderHeaderRepository orderHeaderRepository, IOrderDetailRepository orderDetailRepository, IPatientRepository patientRepository, ApplicationDbContext context, IHubContext<OrderHub> orderHubContext)
        {
            _orderHeaderRepository = orderHeaderRepository;
            _stripeModel = stripeOptions.Value;
            StripeConfiguration.ApiKey = _stripeModel.SecretKey;
            _orderDetailRepository = orderDetailRepository;
            _patientRepository = patientRepository;
            _context = context;
            _orderHubContext = orderHubContext;
        }

        /// <summary>
        /// Creates a Stripe payment session for the specified order.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to create a payment session for.</param>
        /// <returns>A ServiceResult containing the Stripe session DTO with session details.</returns>
        public async Task<ServiceResult<StripeSessionDTO>> CreateStripeSessionAsync(decimal deliveryFee, string accountId)
        {
            var patient = await _context.Patients
                .Include(p => p.Account)
                .Include(p => p.CartItems)
                    .ThenInclude(ci => ci.PharmacyProduct)
                        .ThenInclude(pp => pp.Drug)
                .FirstOrDefaultAsync(p => p.AccountId == accountId);

            if (patient == null || patient.Account == null || patient.CartItems == null || !patient.CartItems.Any())
            {
                return ServiceResult<StripeSessionDTO>.ErrorResult("Patient or cart not found.", ErrorType.NotFound);
            }

            var cartItems = patient.CartItems.ToList();

            var sessionOptions = await BuildSessionOptionsAsync(cartItems, deliveryFee, accountId);
            if (!sessionOptions.Success)
                return ServiceResult<StripeSessionDTO>.ErrorResult(sessionOptions.ErrorMessage, sessionOptions.ErrorType ?? ErrorType.Internal);

            var service = new SessionService();
            var session = await service.CreateAsync(sessionOptions.Data);

            return ServiceResult<StripeSessionDTO>.SuccessResult(new StripeSessionDTO
            {
                SessionId = session.Id,
                Url = session.Url
            });
        }


        /// <summary>
        /// Handles incoming Stripe webhook events from the payment gateway.
        /// </summary>
        /// <param name="request">The HTTP request containing the webhook payload.</param>
        /// <returns>A ServiceResult indicating the success or failure of webhook processing.</returns>
        public async Task<ServiceResult> HandleStripeWebhookAsync(HttpRequest request)
        {
            try
            {
                request.EnableBuffering();

                var json = await new StreamReader(request.Body).ReadToEndAsync();
                request.Body.Position = 0;

                var signatureHeader = request.Headers["Stripe-Signature"];

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signatureHeader,
                    _stripeModel.WebhookSecret,
                    throwOnApiVersionMismatch: false
                );

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session == null)
                        return ServiceResult.ErrorResult("Invalid session object from webhook.", ErrorType.Validation);

                    // Extract metadata
                    var accountId = session.Metadata["accountId"];
                    var deliveryFee = decimal.Parse(session.Metadata["deliveryFee"]);

                    // Get full patient & cart
                    var patient = await _context.Patients
                        .Include(p => p.Account)
                        .Include(p => p.CartItems)
                            .ThenInclude(ci => ci.PharmacyProduct)
                                .ThenInclude(pp => pp.Drug)
                        .FirstOrDefaultAsync(p => p.AccountId == accountId);

                    if (patient == null || !patient.CartItems.Any())
                        return ServiceResult.ErrorResult("Patient or cart not found.", ErrorType.NotFound);

                    var cartItems = patient.CartItems;
                    var subtotal = cartItems.Sum(i => i.Quantity * i.PharmacyProduct.Price);
                    var total = subtotal + deliveryFee;
                    var pharmacyId = cartItems.First().PharmacyId;

                    // Create order
                    var order = new Order
                    {
                        PatientId = patient.PatientId,
                        Name = patient.Name,
                        PhoneNumber = patient.Account.PhoneNumber,
                        Email = patient.Account.Email,
                        Country = patient.Country,
                        Address = patient.Address,
                        TotalPrice = total,
                        OrderDate = DateTime.UtcNow,
                        PharmacyId = pharmacyId,
                        PaymentStatus = SD.PaymentStatusApproved,
                        Status = SD.StatusUnderReview,
                        StatusLastUpdated = DateTime.Now,
                        PaymentMethod = "Stripe",
                        SessionId = session.Id,
                        PaymentIntentId = session.PaymentIntentId,
                        Message = $"New Order Recieved From {patient.Name}."
                    };

                    await _orderHeaderRepository.CreateAndSaveAsync(order);

                    // Add order details
                    foreach (var item in cartItems)
                    {
                        var detail = new OrderDetail
                        {
                            OrderId = order.OrderID,
                            DrugId = item.DrugId,
                            PharmacyId = item.PharmacyId,
                            Quantity = item.Quantity,
                            Price = item.PharmacyProduct.Price
                        };

                        await _orderDetailRepository.CreateAndSaveAsync(detail);
                    }

                    // Save & clean cart
                    await _orderHeaderRepository.SaveAsync();
                    _context.CartItems.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();

                    await _orderHubContext.Clients.Group(pharmacyId.ToString())
                        .SendAsync("NewOrder", new
                        {
                            OrderId = order.OrderID,
                            PaymentMethod = order.PaymentMethod,
                            TotalPrice = total,
                            CreatedAt = DateTime.Now
                        });

                    return ServiceResult.SuccessResult();
                }

                return ServiceResult.SuccessResult(); // Unhandled event types are safely ignored
            }
            catch (StripeException ex)
            {
                return ServiceResult.ErrorResult($"Stripe error: {ex.Message}", ErrorType.Internal);
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult("Webhook processing error: " + ex.Message);
            }
        }


        /// <summary>
        /// Initiates a refund for a Stripe payment using the specified payment intent ID.
        /// </summary>
        /// <param name="paymentIntentId">The unique identifier of the Stripe payment intent to refund.</param>
        /// <returns>A ServiceResult containing a message about the refund status.</returns>
        public async Task<ServiceResult<string>> RefundStripePaymentAsync(string paymentIntentId)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeModel.SecretKey;

                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Reason = RefundReasons.RequestedByCustomer,
                };

                var refundService = new RefundService();
                await refundService.CreateAsync(refundOptions);

                var order = await _orderHeaderRepository.GetAsync(
                o => o.PaymentIntentId == paymentIntentId, tracking: true, x => x.OrderDetails);

                order.PaymentStatus = SD.PaymentStatusRefunded;
                return ServiceResult<string>.SuccessResult("Refund succeeded.");
            }
            catch (StripeException ex)
            {
                return ServiceResult<string>.ErrorResult($"Stripe refund failed: {ex.Message}", ErrorType.Internal);
            }
        }

        /// <summary>
        /// Builds Stripe session options for the given order and order details.
        /// </summary>
        /// <param name="order">The order for which to build session options.</param>
        /// <param name="orderDetails">The details of the order.</param>
        /// <returns>A ServiceResult containing the Stripe session creation options.</returns>
        private async Task<ServiceResult<SessionCreateOptions>> BuildSessionOptionsAsync(ICollection<CartItem> cartItems, decimal deliveryFee, string accountId)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = "http://localhost:4200/client/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "http://localhost:4200/client/cancel",
                Metadata = new Dictionary<string, string>
                {
                    { "accountId", accountId },
                    { "deliveryFee", deliveryFee.ToString() }
                }
            };

            foreach (var item in cartItems)
            {
                var drug = item.PharmacyProduct?.Drug;
                if (drug == null)
                    return ServiceResult<SessionCreateOptions>.ErrorResult("Drug information missing for some cart items.", ErrorType.Validation);

                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = drug.CommonName ?? "Unnamed Drug",
                            Description = drug.Category ?? "No description"
                        },
                        UnitAmount = (long)(item.PharmacyProduct.Price * 100)
                    },
                    Quantity = item.Quantity
                });
            }

            if (deliveryFee > 0)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Delivery Fee",
                            Description = "Standard delivery charge"
                        },
                        UnitAmount = (long)(deliveryFee * 100)
                    },
                    Quantity = 1
                });
            }

            return ServiceResult<SessionCreateOptions>.SuccessResult(options);
        }

    }
}
