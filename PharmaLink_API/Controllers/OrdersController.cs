using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.CartDTO;
using PharmaLink_API.Models.DTO.OrderDTO;
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
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        public OrdersController(IOrderService orderService, IStripeService stripeService, IOrderHeaderRepository orderHeaderRepository)
        {
            _orderService = orderService;
            _stripeService = stripeService;
            _orderHeaderRepository = orderHeaderRepository;
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
                var errorResponse = new
                {
                    success = false,
                    errorType = result.ErrorType.ToString(),
                    message = result.ErrorMessage
                };

                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(errorResponse),
                    ErrorType.Validation => BadRequest(errorResponse),
                    ErrorType.Authorization => Forbid(),
                    ErrorType.Conflict => Conflict(errorResponse),
                    _ => StatusCode(500, errorResponse)
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
        public async Task<ActionResult> CreateCheckoutSession([FromBody] StripeSessionRequestDTO request)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");

            var result = await _stripeService.CreateStripeSessionAsync( request.DeliveryFee, accountId);

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

        [HttpGet("validate-session")]
        public async Task<IActionResult> ValidateStripeSession([FromQuery] string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return BadRequest("Session ID is required.");

            var order = await _orderHeaderRepository.GetAsync(
                o => o.SessionId == sessionId,
                tracking: false,
                x => x.OrderDetails
            );

            if (order == null)
                return NotFound("Order not created yet. Please wait a moment.");

            var result = new
            {
                order.OrderID,
                order.TotalPrice,
                order.Status,
                order.OrderDate,
                order.PaymentStatus
            };

            return Ok(result);
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

        /// <summary>
        /// Retrieves an order summary for the authenticated patient based on their cart items.
        /// </summary>
        /// <returns>Order summary DTO if found; otherwise, NotFound or error response.</returns>
        [Authorize(Roles = "Patient")]
        [HttpGet("order-summary")]
        public async Task<IActionResult> GetOrderSummary()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");

            var result = await _orderService.GetOrderSummaryAsync(accountId);

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

        [HttpGet("PatientOrders")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetAllOrders()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");

            var result = await _orderService.GetPatientOrdersAsync(accountId);
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

        [HttpGet("AdmintOrders")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrdersForAdmin()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");

            var result = await _orderService.GetAdminOrdersAsync(accountId);
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

        //[Authorize(Roles = "Pharmacy,Admin")]
        [HttpGet("analysis")]
        public async Task<IActionResult> GetPharmacyAnalysis(int? pharmacyid)
        {
            string accountId;

            // If id is provided, use it; otherwise, get the accountId from the user claims
            if (pharmacyid.HasValue)
            {
                var pharmacy = await _orderService.GetThePharmacyByIdAsync(pharmacyid.Value);
                if (pharmacy == null)
                    return NotFound("Pharmacy not found.");
                // Assuming the pharmacy object has an AccountId property
                accountId = pharmacy.AccountId;
                if (string.IsNullOrEmpty(accountId))
                    return Forbid("AccountId missing.");
            }
            else
            {
                accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(accountId))
                    return Forbid("AccountId missing.");
            }

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
        //[Authorize(Roles = "Admin")]
        [HttpGet("allOrdersAnalysis")]
        public async Task<IActionResult> GetAllOrdersAnalysis()
        {
            var result = await _orderService.GetAllOrdersAnalysisAsync();
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
        [HttpGet("pharmacies-summary")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPharmaciesSummary()
        {
            var result = await _orderService.GetAllPharmaciesSummaryAsync();
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
