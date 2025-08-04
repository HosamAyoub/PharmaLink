using PharmaLink_API.Core.Results;
using PharmaLink_API.Models.DTO.OrderDTO;

namespace PharmaLink_API.Services.Interfaces
{
    public interface IStripeService
    {
        /// <summary>
        /// Creates a Stripe payment session for the specified order.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to create a payment session for.</param>
        /// <returns>A ServiceResult containing the Stripe session DTO with session details.</returns>
        Task<ServiceResult<StripeSessionDTO>> CreateStripeSessionAsync(int orderId);

        /// <summary>
        /// Handles incoming Stripe webhook events from the payment gateway.
        /// </summary>
        /// <param name="request">The HTTP request containing the webhook payload.</param>
        /// <returns>A ServiceResult indicating the success or failure of webhook processing.</returns>
        Task<ServiceResult> HandleStripeWebhookAsync(HttpRequest request);

        /// <summary>
        /// Initiates a refund for a Stripe payment using the specified payment intent ID.
        /// </summary>
        /// <param name="paymentIntentId">The unique identifier of the Stripe payment intent to refund.</param>
        /// <returns>A ServiceResult containing a message about the refund status.</returns>
        Task<ServiceResult<string>> RefundStripePaymentAsync(string paymentIntentId);
    }
}
