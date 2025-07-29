using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.CartDTO;
using PharmaLink_API.Services.Interfaces;
using System.Security.Claims;

namespace PharmaLink_API.Controllers
{
    [Authorize(Roles = "Patient")]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<CartItemSummaryDTO>> GetCartSummary()
        {
            // Extract the account ID from the JWT token
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid user ID in token.");

            // Get the cart summary using the service
            var cartSummary = await _cartService.GetCartSummaryAsync(accountId);
            if (cartSummary == null)
                return NotFound("Your cart is empty or patient not found.");

            return Ok(cartSummary);
        }

        [HttpPost("AddToCart")]
        public async Task<ActionResult> AddToCart([FromBody] AddToCartDTO cartItem)
        {
            // Validate the input DTO: ensure it's not null and contains a valid PharmacyId
            if (cartItem == null || cartItem.PharmacyId == 0)
                return BadRequest("Cart item is invalid");

            // Extract the account ID from the JWT token
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid user ID in token.");

            try
            {
                var result = await _cartService.AddToCartAsync(accountId, cartItem);
                return Ok(new { cartItem = result.cartItem, totalCount = result.totalCount });
            }
            catch (ArgumentException ex)
            {
                //Patient not found or invalid data
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                //Business rule violations (e.g., trying to add from a different pharmacy)
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveItemFromCart([FromBody] CartUpdateDTO cartUpdateDTO)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid user ID in token.");

            try
            {
                await _cartService.RemoveItemFromCartAsync(accountId, cartUpdateDTO);
                return Ok(new { message = "Item removed from cart." });
            }
            catch (ArgumentException ex)
            {
                // patient not found
                return NotFound(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                // item not found in cart
                return NotFound(ex.Message);
            }
        }

        [HttpPost("plus")]
        public async Task<ActionResult<CartItem>> IncrementCartItem([FromBody] CartUpdateDTO cartUpdateDTO)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid user ID in token.");

            try
            {
                var result = await _cartService.IncrementCartItemAsync(accountId, cartUpdateDTO);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("minus")]
        public async Task<ActionResult<CartItem>> DecrementCartItem([FromBody] CartUpdateDTO cartUpdateDTO)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid user ID in token.");

            try
            {
                var result = await _cartService.DecrementCartItemAsync(accountId, cartUpdateDTO);
                if (result == null)
                    return Ok(new { message = "Item removed from cart" });

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid user ID in token.");

            try
            {
                await _cartService.ClearCartAsync(accountId);
                return Ok(new { message = "Cart has been cleared successfully." });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
