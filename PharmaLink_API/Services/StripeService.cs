using Microsoft.Extensions.Options;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Core.Results;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.OrderDTO;
using PharmaLink_API.Repository;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;

namespace PharmaLink_API.Services
{
    public class StripeService : IStripeService
    {
        private readonly StripeModel _stripeModel;
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        public StripeService(IOptions<StripeModel> stripeOptions, IOrderHeaderRepository orderHeaderRepository, IOrderDetailRepository orderDetailRepository)
        {
            _orderHeaderRepository = orderHeaderRepository;
            _stripeModel = stripeOptions.Value;
            StripeConfiguration.ApiKey = _stripeModel.SecretKey;
            _orderDetailRepository = orderDetailRepository;
        }

        public async Task<ServiceResult<StripeSessionDTO>> CreateStripeSessionAsync(int orderId)
        {
            var order = await _orderHeaderRepository.GetAsync(
                o => o.OrderID == orderId, tracking: true, x => x.OrderDetails
            );

            if (order == null)
                return ServiceResult<StripeSessionDTO>.ErrorResult("Order not found.", ErrorType.NotFound);

            var orderDetails = await _orderDetailRepository.GetAllAsync(
                od => od.OrderId == orderId, x => x.PharmacyProduct.Drug
            );

            var sessionOptions = await BuildSessionOptionsAsync(order, orderDetails);
            if (!sessionOptions.Success)
                return ServiceResult<StripeSessionDTO>.ErrorResult(sessionOptions.ErrorMessage, sessionOptions.ErrorType ?? ErrorType.Internal);

            var service = new SessionService();
            var session = await service.CreateAsync(sessionOptions.Data);

            _orderHeaderRepository.UpdateStripePaymentID(orderId, session.Id, session.PaymentIntentId);
            await _orderHeaderRepository.SaveAsync();

            return ServiceResult<StripeSessionDTO>.SuccessResult(new StripeSessionDTO
            {
                SessionId = session.Id,
                Url = session.Url
            });
        }

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

                    var sessionService = new SessionService();
                    var fullSession = await sessionService.GetAsync(session.Id);

                    var order = await _orderHeaderRepository.GetAsync(o => o.SessionId == session.Id);
                    if (order == null)
                        return ServiceResult.ErrorResult($"No order found for session ID {session.Id}.", ErrorType.NotFound);

                    order.PaymentStatus = SD.PaymentStatusApproved;
                    order.PaymentIntentId = fullSession.PaymentIntentId;

                    await _orderHeaderRepository.SaveAsync();

                    return ServiceResult.SuccessResult();
                }

                return ServiceResult.SuccessResult();
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


        private async Task<ServiceResult<SessionCreateOptions>> BuildSessionOptionsAsync(PharmaLink_API.Models.Order order, IEnumerable<OrderDetail> orderDetails)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"http://localhost:4200/client/success?orderId={order.OrderID}",
                CancelUrl = "http://localhost:4200/client/cancel"
            };

            foreach (var item in orderDetails)
            {
                var drug = item.PharmacyProduct?.Drug;
                if (drug == null)
                    return ServiceResult<SessionCreateOptions>.ErrorResult("Drug information missing for some cart items.", ErrorType.Validation);

                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = drug.CommonName ?? "Unnamed Drug",
                            Description = drug.Description ?? "No description"
                        },
                        UnitAmount = (long)(item.Price * 100)
                    },
                    Quantity = item.Quantity
                });
            }

            return ServiceResult<SessionCreateOptions>.SuccessResult(options);
        }

    }
}
