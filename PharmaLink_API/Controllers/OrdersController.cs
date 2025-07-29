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
        public OrdersController( IOrderService orderService, IStripeService stripeService)
        {
            _orderService = orderService;
            _stripeService = stripeService;
        }

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

        [HttpPost("CreateCheckoutSession")]
        public async Task<ActionResult> CreateCheckoutSession([FromBody]int orderId)
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

        [Authorize(Roles = "Pharmacy")]
        [HttpPost("accept/{orderId}")]
        public async Task<IActionResult> AcceptOrder(int orderId)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Forbid("AccountId missing.");

            var result = await _orderService.AcceptOrderAsync(orderId, accountId);

            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result.ErrorMessage),
                    ErrorType.Validation => BadRequest(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            return Ok(new { Message = $"Order #{orderId} has been accepted." });
        }

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

    }
}
