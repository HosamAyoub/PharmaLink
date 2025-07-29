using PharmaLink_API.Core.Results;
using PharmaLink_API.Models.DTO.CartDTO;
using PharmaLink_API.Models.DTO.OrderDTO;

namespace PharmaLink_API.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ServiceResult<OrderResponseDTO>> SubmitOrderAsync(string accountId, SubmitOrderRequestDTO dto);
        Task<ServiceResult<string>> CancelOrderAsync(string accountId, int orderId);
        Task<ServiceResult> AcceptOrderAsync(int orderId, string accountId);
        Task<ServiceResult> RejectOrderAsync(int orderId, string accountId);
        Task<ServiceResult<IEnumerable<PharmacyOrderDTO>>> GetPharmacyOrdersAsync(string accountId);
    }
}
