using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.CartDTO;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;

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
        private readonly IOrderService _orderService;
        private readonly IStripeService _stripeService;
        public OrdersController(IOrderService orderService, IStripeService stripeService)
        {
            _orderService = orderService;
            _stripeService = stripeService;
        }

        /// <summary>
        /// Submits a new order for the authenticated patient.
        /// </summary>
        /// <param name="dto">Order submission details.</param>
        /// <returns>Order response DTO if successful.</returns>
        [Authorize(Roles = "Patient")]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitOrder([FromBody] SubmitOrderRequestDTO dto)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");

            var result = await _orderService.SubmitOrderAsync(accountId, dto);

            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result.ErrorMessage),
                    ErrorType.Validation => BadRequest(result.ErrorMessage),
                    ErrorType.Authorization => Forbid(result.ErrorMessage),
                    ErrorType.Conflict => Conflict(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Creates a Stripe checkout session for the specified order.
        /// </summary>
        /// <param name="orderId">Order ID to create session for.</param>
        /// <returns>Stripe session DTO if successful.</returns>
        [HttpPost("CreateCheckoutSession")]
        public async Task<ActionResult> CreateCheckoutSession([FromBody] int orderId)
        {
            var result = await _stripeService.CreateStripeSessionAsync(orderId);

            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result.ErrorMessage),
                    ErrorType.Validation => BadRequest(result.ErrorMessage),
                    ErrorType.Authorization => Forbid(result.ErrorMessage),
                    ErrorType.Conflict => Conflict(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Handles Stripe webhook events for payment processing.
        /// </summary>
        /// <returns>Status of webhook processing.</returns>
        [HttpPost("stripe-webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var result = await _stripeService.HandleStripeWebhookAsync(Request);

            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result.ErrorMessage),
                    ErrorType.Validation => BadRequest(result.ErrorMessage),
                    ErrorType.Authorization => Forbid(result.ErrorMessage),
                    ErrorType.Conflict => Conflict(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            return Ok();
        }

        /// <summary>
        /// Cancels an existing order for the authenticated patient.
        /// </summary>
        /// <param name="orderId">Order ID to cancel.</param>
        /// <returns>Cancellation status message.</returns>
        [Authorize(Roles = "Patient")]
        [HttpPost("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");

            var result = await _orderService.CancelOrderAsync(accountId, orderId);

            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result.ErrorMessage),
                    ErrorType.Validation => BadRequest(result.ErrorMessage),
                    ErrorType.Authorization => Forbid(result.ErrorMessage),
                    ErrorType.Conflict => Conflict(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            return Ok(new { Message = result.Data });
        }

        //******************Pharmacy Only Endpoints******************//

        /// <summary>
        /// Retrieves all orders for the authenticated pharmacy.
        /// </summary>
        /// <returns>Collection of pharmacy order DTOs.</returns>
        [Authorize(Roles = "Pharmacy")]
        [HttpGet("orders")]
        public async Task<IActionResult> GetPharmacyOrders()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Forbid("AccountId missing.");

            var result = await _orderService.GetPharmacyOrdersAsync(accountId);

            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            return Ok(result.Data);
        }


        /// <summary>
        /// Sets the specified order to "In Review" status for the authenticated pharmacy.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to review.</param>
        /// <returns>Order details DTO if successful, or an error response.</returns>
        [Authorize(Roles = "Pharmacy")]
        [HttpPost("reviewing/{orderId}")]
        public async Task<IActionResult> OrderInReview(int orderId)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Forbid("AccountId missing.");

            var result = await _orderService.ReviewingOrderAsync(orderId, accountId);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result.ErrorMessage),
                    ErrorType.Validation => BadRequest(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }
            return Ok(result.Data);
        }

        /// <summary>
        /// Sets the specified order to "Pending" status for the authenticated pharmacy.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to update.</param>
        /// <returns>Status message indicating the order is pending, or an error response.</returns>
        [Authorize(Roles = "Pharmacy")]
        [HttpPost("pending/{orderId}")]
        public async Task<IActionResult> OrderPending(int orderId)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Forbid("AccountId missing.");

            var result = await _orderService.PendingOrderAsync(orderId, accountId);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result.ErrorMessage),
                    ErrorType.Validation => BadRequest(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }
            return Ok(new { Message = $"Order #{orderId} is pending." });
        }

        /// <summary>
        /// Accepts an order for the authenticated pharmacy.
        /// </summary>
        /// <param name="orderId">Order ID to accept.</param>
        /// <returns>Acceptance status message.</returns>
        [Authorize(Roles = "Pharmacy")]
        [HttpPost("outForDelivery/{orderId}")]
        public async Task<IActionResult> OrderOutForDelivery(int orderId)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Forbid("AccountId missing.");

            var result = await _orderService.OutForDeliveryOrderAsync(orderId, accountId);

            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result.ErrorMessage),
                    ErrorType.Validation => BadRequest(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            return Ok(new { Message = $"Order #{orderId} is out for delivery." });
        }

        /// <summary>
        /// Marks the specified order as delivered for the authenticated pharmacy.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to mark as delivered.</param>
        /// <returns>Status message indicating the order has been delivered, or an error response.</returns>
        [Authorize(Roles = "Pharmacy")]
        [HttpPost("delivered/{orderId}")]
        public async Task<IActionResult> OrderDelivered(int orderId)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Forbid("AccountId missing.");

            var result = await _orderService.OrderDeliveredAsync(orderId, accountId);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result.ErrorMessage),
                    ErrorType.Validation => BadRequest(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }
            return Ok(new { Message = $"Order #{orderId} is in review." });
        }

        /// <summary>
        /// Rejects an order for the authenticated pharmacy.
        /// </summary>
        /// <param name="orderId">Order ID to reject.</param>
        /// <returns>Rejection status message.</returns>
        [Authorize(Roles = "Pharmacy")]
        [HttpPost("reject/{orderId}")]
        public async Task<IActionResult> RejectOrder(int orderId)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Forbid("AccountId missing.");

            var result = await _orderService.RejectOrderAsync(orderId, accountId);

            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result.ErrorMessage),
                    ErrorType.Validation => BadRequest(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            return Ok(new { Message = $"Order #{orderId} has been rejected." });
        }

        /// <summary>
        /// Searches pharmacy orders for the authenticated pharmacy using a query string.
        /// </summary>
        /// <param name="query">Search term to filter orders.</param>
        /// <returns>List of matching pharmacy order DTOs or error response.</returns>
        [Authorize(Roles = "Pharmacy")]
        [HttpGet("search")]
        public async Task<IActionResult> SearchOrders(string query)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Forbid("AccountId missing.");

            var result = await _orderService.SearchOrdersAsync(accountId, query);

            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result.ErrorMessage),
                    ErrorType.Validation => BadRequest(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            return Ok(result);
        }

        /// <summary>
        /// Filters pharmacy orders for the authenticated pharmacy by status.
        /// </summary>
        /// <param name="status">Order status to filter by.</param>
        /// <returns>List of pharmacy order DTOs matching the status or error response.</returns>
        [Authorize(Roles = "Pharmacy")]
        [HttpGet("filter")]
        public async Task<IActionResult> FilterOrdersByStatus(string status)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Forbid("AccountId missing.");

            var result = await _orderService.FilterOrdersByStatusAsync(accountId, status);

            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result.ErrorMessage),
                    ErrorType.Validation => BadRequest(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            return Ok(result); // Assuming result is already wrapped in ServiceResult<List<PharmacyOrderDTO>>
        }

        [Authorize(Roles = "Pharmacy")]
        [HttpGet("analysis")]
        public async Task<IActionResult> GetPharmacyAnalysis()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Forbid("AccountId missing.");

            var result = await _orderService.GetPharmacyAnalysisAsync(accountId);

            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            return Ok(result.Data);
        }

    }
}
