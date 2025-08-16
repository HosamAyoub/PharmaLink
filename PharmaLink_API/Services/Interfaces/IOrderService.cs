using PharmaLink_API.Core.Results;
using PharmaLink_API.Models;
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
        /// Retrieves an order summary for the specified account based on cart items.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <returns>A ServiceResult containing the order summary DTO if found.</returns>
        Task<ServiceResult<OrderSummaryDTO>> GetOrderSummaryAsync(string accountId);

        /// <summary>
        /// Retrieves the details of an order for review by the specified account.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to review.</param>
        /// <param name="accountId">The unique identifier of the account reviewing the order.</param>
        /// <returns>A ServiceResult containing the order details DTO if found.</returns>
        Task<ServiceResult<OrderDetailsDTO>> ReviewingOrderAsync(int orderId, string accountId);

        /// <summary>
        /// Marks an order as pending for the specified account.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to mark as pending.</param>
        /// <param name="accountId">The unique identifier of the account performing the action.</param>
        /// <returns>A ServiceResult indicating the success or failure of the operation.</returns>
        Task<ServiceResult> PendingOrderAsync(int orderId, string accountId);

        /// <summary>
        /// Marks an order as delivered for the specified account.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to mark as delivered.</param>
        /// <param name="accountId">The unique identifier of the account performing the action.</param>
        /// <returns>A ServiceResult indicating the success or failure of the operation.</returns>
        Task<ServiceResult> OrderDeliveredAsync(int orderId, string accountId);

        /// <summary>
        /// Updates the status of an order to 'Out for Delivery' for the specified account and order ID.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to update.</param>
        /// <param name="accountId">The unique identifier of the account performing the action.</param>
        /// <returns>A ServiceResult indicating the success or failure of the operation.</returns>
        Task<ServiceResult> OutForDeliveryOrderAsync(int orderId, string accountId);

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

        /// <summary>
        /// Searches pharmacy orders for the specified account using a query string.
        /// </summary>
        /// <param name="accountId">The unique identifier of the pharmacy account.</param>
        /// <param name="query">The search query to filter orders.</param>
        /// <returns>A ServiceResult containing a list of matching pharmacy order DTOs.</returns>
        Task<ServiceResult<List<PharmacyOrderDTO>>> SearchOrdersAsync(string accountId, string query);

        /// <summary>
        /// Filters pharmacy orders by status for the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the pharmacy account.</param>
        /// <param name="status">The status to filter orders by.</param>
        /// <returns>A ServiceResult containing a list of pharmacy order DTOs with the specified status.</returns>
        Task<ServiceResult<List<PharmacyOrderDTO>>> FilterOrdersByStatusAsync(string accountId, string status);

        Task<ServiceResult<List<PatientOrdersDTO>>> GetPatientOrdersAsync(string accountId);
        Task<ServiceResult<List<PatientOrdersDTO>>> GetAdminOrdersAsync(string accountId);

        Task<Pharmacy?> GetThePharmacyByIdAsync(int id);

        Task<ServiceResult<PharmacyAnalysisDTO>> GetPharmacyAnalysisAsync(string accountId);

        Task<ServiceResult<PharmacyAnalysisDTO>> GetAllOrdersAnalysisAsync();

        /// <summary>
        /// Returns a summary for all pharmacies including orders, revenue, and stock count.
        /// </summary>
        Task<ServiceResult<PharmacySummaryDTO>> GetAllPharmaciesSummaryAsync();
    }
}
