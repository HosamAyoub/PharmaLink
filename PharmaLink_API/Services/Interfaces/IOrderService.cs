using PharmaLink_API.Core.Results;
using PharmaLink_API.Models.DTO.CartDTO;
using PharmaLink_API.Models.DTO.OrderDTO;

namespace PharmaLink_API.Services.Interfaces
{
    public interface IOrderService
    {
        /// <summary>
        /// Submits a new order for the specified account using the provided order details.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account placing the order.</param>
        /// <param name="dto">The DTO containing order submission details.</param>
        /// <returns>A ServiceResult containing the order response DTO if successful.</returns>
        Task<ServiceResult<OrderResponseDTO>> SubmitOrderAsync(string accountId, SubmitOrderRequestDTO dto);

        /// <summary>
        /// Cancels an existing order for the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account requesting cancellation.</param>
        /// <param name="orderId">The unique identifier of the order to cancel.</param>
        /// <returns>A ServiceResult containing a message about the cancellation status.</returns>
        Task<ServiceResult<string>> CancelOrderAsync(string accountId, int orderId);

        /// <summary>
        /// Accepts an order for the specified account and order ID.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to accept.</param>
        /// <param name="accountId">The unique identifier of the account accepting the order.</param>
        /// <returns>A ServiceResult indicating the success or failure of the operation.</returns>
        Task<ServiceResult> AcceptOrderAsync(int orderId, string accountId);

        /// <summary>
        /// Rejects an order for the specified account and order ID.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to reject.</param>
        /// <param name="accountId">The unique identifier of the account rejecting the order.</param>
        /// <returns>A ServiceResult indicating the success or failure of the operation.</returns>
        Task<ServiceResult> RejectOrderAsync(int orderId, string accountId);

        /// <summary>
        /// Retrieves all pharmacy orders associated with the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the pharmacy account.</param>
        /// <returns>A ServiceResult containing a collection of pharmacy order DTOs.</returns>
        Task<ServiceResult<IEnumerable<PharmacyOrderDTO>>> GetPharmacyOrdersAsync(string accountId);
    }
}
