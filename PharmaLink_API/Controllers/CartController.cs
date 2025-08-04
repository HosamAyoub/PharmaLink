using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.CartDTO;
using PharmaLink_API.Services.Interfaces;
using System.Security.Claims;

namespace PharmaLink_API.Controllers
{
    [Authorize]
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

        /// <summary>
        /// Retrieves a summary of the current patient's cart, including items and order summary.
        /// </summary>
        /// <returns>Cart summary DTO if found; otherwise, NotFound or Unauthorized.</returns>
        [HttpGet("summary")]
        public async Task<ActionResult<CartItemSummaryDTO>> GetCartSummary()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid user ID in token.");

            var cartSummary = await _cartService.GetCartSummaryAsync(accountId);
            if (cartSummary == null)
                return NotFound("Your cart is empty or patient not found.");

            return Ok(cartSummary);
        }

        /// <summary>
        /// Adds an item to the current patient's cart.
        /// </summary>
        /// <param name="cartItem">DTO containing item details to add.</param>
        /// <returns>Added cart item and total count, or error response.</returns>
        [HttpPost("AddToCart")]
        public async Task<ActionResult> AddToCart([FromBody] AddToCartDTO cartItem)
        {
            if (cartItem == null || cartItem.PharmacyId == 0)
                return BadRequest("Cart item is invalid");

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
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Removes a specific item from the current patient's cart.
        /// </summary>
        /// <param name="cartUpdateDTO">DTO specifying which item to remove.</param>
        /// <returns>Success or error response.</returns>
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
                return NotFound(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Increments the quantity of a specific item in the current patient's cart.
        /// </summary>
        /// <param name="cartUpdateDTO">DTO specifying which item to increment.</param>
        /// <returns>Updated cart item or error response.</returns>
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

        /// <summary>
        /// Decrements the quantity of a specific item in the current patient's cart.
        /// </summary>
        /// <param name="cartUpdateDTO">DTO specifying which item to decrement.</param>
        /// <returns>Updated cart item, or message if item removed, or error response.</returns>
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

        /// <summary>
        /// Clears all items from the current patient's cart.
        /// </summary>
        /// <returns>Success or error response.</returns>
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
