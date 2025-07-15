using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.CartDTO;
using PharmaLink_API.Repository.IRepository;

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public CartController(ICartRepository cartRepository, IUserRepository userRepository, IMapper mapper)
        {
            _cartRepository = cartRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCartSummary(int userId)
        {
            var cartItems = await _cartRepository.GetAllAsync(u => u.UserId == userId, x => x.PharmacyStocks);
            var user = await _userRepository.GetAsync(u => u.UserID == userId, true, x => x.Account);

            if (cartItems == null || !cartItems.Any())
            {
                return NotFound("No items found in the cart");
            }
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.Account == null)
            {
                return NotFound("User account information is missing.");
            }
            var cartDto = new CartItemSummaryDTO
            {
                cartItems = cartItems.Select(c => new AddToCartDTO
                {
                    UserId = c.UserId,
                    DrugId = c.DrugId,
                    PharmacyId = c.PharmacyId,
                    Quantity = c.Quantity,
                    Price = c.Price
                }),
                order = new Order
                {
                    Name = user.Name,
                    PhoneNumber = user.MobileNumber,
                    Email = user.Account.Email,
                    Country = user.Country,
                    Address = user.Address,
                    TotalPrice = cartItems.Sum(c => c.Price * c.Quantity)
                }
            };

            return Ok(cartDto);
        }

        [HttpPost("AddToCart")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AddToCartDTO>> AddToCart([FromBody] AddToCartDTO cartItem)
        {
            if (cartItem == null || cartItem.PharmacyId == null || cartItem.UserId == null || cartItem.UserId == null)
            {
                return BadRequest("Cart item cannot be null");
            }

            var existingCartItem = await _cartRepository.GetAsync(
                u => u.UserId == cartItem.UserId && u.DrugId == cartItem.DrugId && u.PharmacyId == cartItem.PharmacyId);

            CartItem finalCartItem;
            if (existingCartItem == null)
            {
                var newCartItem = _mapper.Map<CartItem>(cartItem);
                await _cartRepository.CreateAsync(newCartItem);
                finalCartItem = newCartItem;
            }
            else
            {
                _cartRepository.IncrementCount(existingCartItem, cartItem.Quantity);
                finalCartItem = existingCartItem;
            }
            await _cartRepository.SaveAsync();

            var cartList = await _cartRepository.GetAllAsync(u => u.UserId == cartItem.UserId);
            var totalCount = cartList.Count;


            return Ok(new { cartItem = finalCartItem, totalCount });
        }

        [HttpPost("plus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartItem>> IncrementCartItem(int userId, int drugId, int pharmacyId)
        {
            var cartItem = await _cartRepository.GetAsync(u => u.UserId == userId && u.DrugId == drugId && u.PharmacyId == pharmacyId);
            if (cartItem == null)
            {
                return NotFound("Cart item not found");
            }
            _cartRepository.IncrementCount(cartItem, 1);
            await _cartRepository.SaveAsync();
            return Ok(cartItem);
        }

        [HttpPost("minus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartItem>> DecrementCartItem(int userId, int drugId, int pharmacyId)
        {
            var cartItem = await _cartRepository.GetAsync(u => u.UserId == userId && u.DrugId == drugId && u.PharmacyId == pharmacyId);
            if (cartItem == null)
            {
                return NotFound("Cart item not found");
            }
            if (cartItem.Quantity <= 1)
            {
                await _cartRepository.RemoveAsync(cartItem);
                await _cartRepository.SaveAsync();
                return Ok(new { message = "Item removed from cart" });
            }
            _cartRepository.DecrementCount(cartItem, 1);
            await _cartRepository.SaveAsync();
            return Ok(cartItem);
        }

        [HttpDelete("remove/{userId}/{drugId}/{pharmacyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RemoveFromCart(int userId, int drugId, int pharmacyId)
        {
            var cartItem = await _cartRepository.GetAsync(
                u => u.UserId == userId && u.DrugId == drugId && u.PharmacyId == pharmacyId
            );

            if (cartItem == null)
            {
                return NotFound(new { message = "Cart item not found" });
            }

            await _cartRepository.RemoveAsync(cartItem);
            await _cartRepository.SaveAsync();

            return Ok(new
            {
                message = "Item successfully removed from cart",
                userId,
                drugId,
                pharmacyId
            });
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItems(int userId)
        {
            var cartItems = await _cartRepository.GetAllAsync(u => u.UserId == userId);
            if (cartItems == null || !cartItems.Any())
            {
                return NotFound("No items found in the cart");
            }
            return Ok(cartItems);
        }

    }
}
