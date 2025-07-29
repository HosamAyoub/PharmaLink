using PharmaLink_API.Core.Results;
using PharmaLink_API.Models.DTO.OrderDTO;

namespace PharmaLink_API.Services.Interfaces
{
    public interface IStripeService
    {
        Task<ServiceResult<StripeSessionDTO>> CreateStripeSessionAsync(int orderId);
        Task<ServiceResult> HandleStripeWebhookAsync(HttpRequest request);
        Task<ServiceResult<string>> RefundStripePaymentAsync(string paymentIntentId);
    }
}
