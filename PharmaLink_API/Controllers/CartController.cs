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
        private readonly IPharmacyStockRepository _pharmacyStockRepository;
        private readonly IMapper _mapper;
        public CartController(ICartRepository cartRepository, IUserRepository userRepository, IMapper mapper, IPharmacyStockRepository pharmacyStockRepository)
        {
            _cartRepository = cartRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _pharmacyStockRepository = pharmacyStockRepository;
        }

        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartItemSummaryDTO>> GetCartSummary(int userId)
        {
            var cartItems = await _cartRepository.GetAllAsync(
                u => u.UserId == userId,
                x => x.PharmacyStocks!.Drug,
                x => x.PharmacyStocks!.Pharmacy
            );

            var user = await _userRepository.GetAsync(u => u.UserID == userId, true, x => x.Account);

            if (cartItems == null || !cartItems.Any())
                return NotFound("No items found in the cart");

            if (user == null || user.Account == null)
                return NotFound("User or user account not found.");

            var deliveryFee = 4.99m;

            var cartItemDtos = cartItems.Select(c => new CartItemDetailsDTO
            {
                drugId = c.DrugId,
                PharmacyId = c.PharmacyId,
                DrugName = c.PharmacyStocks?.Drug?.CommonName ?? "Unknown Drug",
                PharmacyName = c.PharmacyStocks?.Pharmacy?.Name ?? "Unknown Pharmacy",
                ImageUrl = c.PharmacyStocks?.Drug?.Drug_UrlImg ?? "", 
                UnitPrice = c.Price,
                Quantity = c.Quantity
            }).ToList();

            var subtotal = cartItemDtos.Sum(x => x.UnitPrice * x.Quantity);

            var orderDto = new OrderSummaryDTO
            {
                Name = user.Name,
                PhoneNumber = user.MobileNumber,
                Email = user.Account.Email,
                Address = user.Address,
                Country = user.Country,
                Subtotal = subtotal,
                DeliveryFee = deliveryFee
            };

            var cartDto = new CartItemSummaryDTO
            {
                CartItems = cartItemDtos,
                OrderSummary = orderDto
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

            var stockExists = await _pharmacyStockRepository.GetAsync(s => s.DrugId == cartItem.DrugId && s.PharmacyId == cartItem.PharmacyId);

            if (stockExists == null)
            {
                return BadRequest("This drug is not available in the selected pharmacy.");
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

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveItemFromCart([FromBody] CartUpdateDTO dto)
        {
            var cartItem = await _cartRepository.GetAsync(c =>
                c.UserId == dto.UserId &&
                c.DrugId == dto.DrugId &&
                c.PharmacyId == dto.PharmacyId);

            if (cartItem == null)
                return NotFound("Item not found in cart.");

            await _cartRepository.RemoveAsync(cartItem);
            await _cartRepository.SaveAsync();

            return Ok(new { message = "Item removed from cart." });
        }

        [HttpPost("plus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartItem>> IncrementCartItem([FromBody] CartUpdateDTO cartUpdateDTO)
        {
            if (cartUpdateDTO == null || cartUpdateDTO.UserId <= 0 || cartUpdateDTO.DrugId <= 0 || cartUpdateDTO.PharmacyId <= 0)
            {
                return BadRequest("Invalid cart update request");
            }
            var userId = cartUpdateDTO.UserId;
            var drugId = cartUpdateDTO.DrugId;
            var pharmacyId = cartUpdateDTO.PharmacyId;

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
        public async Task<ActionResult<CartItem>> DecrementCartItem([FromBody] CartUpdateDTO cartUpdateDTO)
        {
            if (cartUpdateDTO == null || cartUpdateDTO.UserId <= 0 || cartUpdateDTO.DrugId <= 0 || cartUpdateDTO.PharmacyId <= 0)
            {
                return BadRequest("Invalid cart update request");
            }
            var cartItem = await _cartRepository.GetAsync(u => u.UserId == cartUpdateDTO.UserId && u.DrugId == cartUpdateDTO.DrugId && u.PharmacyId == cartUpdateDTO.PharmacyId);
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


        //[HttpGet("{userId}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItems(int userId)
        //{
        //    var cartItems = await _cartRepository.GetAllAsync(u => u.UserId == userId);
        //    if (cartItems == null || !cartItems.Any())
        //    {
        //        return NotFound("No items found in the cart");
        //    }
        //    return Ok(cartItems);
        //}

    }
}
