using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.CartDTO;
using PharmaLink_API.Repository.Interfaces;
using System.Security.Claims;

namespace PharmaLink_API.Controllers
{
    [Authorize(Roles = "Patient")]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IPharmacyStockRepository _pharmacyStockRepository;
        private readonly IMapper _mapper;
        public CartController(ICartRepository cartRepository, IPatientRepository patientRepository, IMapper mapper, IPharmacyStockRepository pharmacyStockRepository)
        {
            _cartRepository = cartRepository;
            _patientRepository = patientRepository;
            _mapper = mapper;
            _pharmacyStockRepository = pharmacyStockRepository;
        }

        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartItemSummaryDTO>> GetCartSummary()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid user ID in token.");
            var patient = await _patientRepository.GetAsync(
                u => u.AccountId == accountId,
                true,
                x => x.Account
            );

            if (patient == null)
                return NotFound("Patient not found.");

            int patientId = patient.PatientId;

            var cartItems = await _cartRepository.GetAllAsync(
                u => u.PatientId == patientId,
                x => x.PharmacyProduct!.Drug,
                x => x.PharmacyProduct!.Pharmacy
            );

            if (cartItems == null || !cartItems.Any())
                return NotFound("Your cart is empty.");

            var deliveryFee = 4.99m;

            var cartItemDtos = cartItems.Select(c => new CartItemDetailsDTO
            {
                DrugId = c.DrugId,
                PharmacyId = c.PharmacyId,
                DrugName = c.PharmacyProduct?.Drug?.CommonName ?? "Unknown Drug",
                PharmacyName = c.PharmacyProduct?.Pharmacy?.Name ?? "Unknown Pharmacy",
                ImageUrl = c.PharmacyProduct?.Drug?.Drug_UrlImg ?? "",
                UnitPrice = c.Price,
                Quantity = c.Quantity
            }).ToList();

            var subtotal = cartItemDtos.Sum(x => x.UnitPrice * x.Quantity);

            var orderDto = new OrderSummaryDTO
            {
                Name = patient.Name,
                PhoneNumber = patient.Account.PhoneNumber,
                Email = patient.Account.Email,
                Address = patient.Address,
                Country = patient.Country,
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
        public async Task<ActionResult> AddToCart([FromBody] AddToCartDTO cartItem)
        {
            if (cartItem == null || cartItem.PharmacyId == 0)
                return BadRequest("Cart item is invalid");

            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid user ID in token.");

            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null)
                return NotFound("Patient not found.");

            int patientId = patient.PatientId;

            var stockExists = await _pharmacyStockRepository.GetAsync(
                s => s.DrugId == cartItem.DrugId && s.PharmacyId == cartItem.PharmacyId);

            if (stockExists == null)
                return BadRequest("This drug is not available in the selected pharmacy.");

            decimal stockPrice = stockExists.Price;

            var existingCartItem = await _cartRepository.GetAsync(
                u => u.PatientId == patientId && u.DrugId == cartItem.DrugId && u.PharmacyId == cartItem.PharmacyId);

            var cartList = await _cartRepository.GetAllAsync(u => u.PatientId == patientId);

            if (cartList.Any())
            {
                var pharmacyInCart = cartList.First().PharmacyId;
                if (pharmacyInCart != cartItem.PharmacyId)
                {
                    return BadRequest("You can only add drugs from one pharmacy at a time.");
                }
            }

            CartItem finalCartItem;
            if (existingCartItem == null)
            {
                var newCartItem = _mapper.Map<CartItem>(cartItem);
                newCartItem.PatientId = patientId;
                newCartItem.Price = stockPrice;
                await _cartRepository.CreateAsync(newCartItem);
                finalCartItem = newCartItem;
            }
            else
            {
                _cartRepository.IncrementCount(existingCartItem, cartItem.Quantity);
                finalCartItem = existingCartItem;
            }

            await _cartRepository.SaveAsync();

            var responseDTO = _mapper.Map<CartItemResponseDTO>(finalCartItem);

            //var cartList = await _cartRepository.GetAllAsync(u => u.PatientId == patientId);
            var totalCount = cartList.Count;

            return Ok(new { cartItem = responseDTO, totalCount });
        }

        [HttpPost("remove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveItemFromCart([FromBody] CartUpdateDTO dto)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid user ID in token.");

            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null)
                return NotFound("Patient not found.");

            int patientId = patient.PatientId;

            var cartItem = await _cartRepository.GetAsync(c =>
                c.PatientId == patientId &&
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
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid user ID in token.");

            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null)
                return NotFound("Patient not found.");

            int patientId = patient.PatientId;


            if (cartUpdateDTO == null ||  cartUpdateDTO.DrugId <= 0 || cartUpdateDTO.PharmacyId <= 0)
            {
                return BadRequest("Invalid cart update request");
            }
            var drugId = cartUpdateDTO.DrugId;
            var pharmacyId = cartUpdateDTO.PharmacyId;

            var cartItem = await _cartRepository.GetAsync(u => u.PatientId == patientId && u.DrugId == drugId && u.PharmacyId == pharmacyId);
            if (cartItem == null)
            {
                return NotFound("Cart item not found");
            }
            _cartRepository.IncrementCount(cartItem, 1);
            await _cartRepository.SaveAsync();
            return Ok(_mapper.Map<CartItemResponseDTO>(cartItem));
        }

        [HttpPost("minus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartItem>> DecrementCartItem([FromBody] CartUpdateDTO cartUpdateDTO)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid user ID in token.");

            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null)
                return NotFound("Patient not found.");

            int patientId = patient.PatientId;

            if (cartUpdateDTO == null || cartUpdateDTO.DrugId <= 0 || cartUpdateDTO.PharmacyId <= 0)
            {
                return BadRequest("Invalid cart update request");
            }
            var cartItem = await _cartRepository.GetAsync(u => u.PatientId == patientId && u.DrugId == cartUpdateDTO.DrugId && u.PharmacyId == cartUpdateDTO.PharmacyId);
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
            return Ok(_mapper.Map<CartItemResponseDTO>(cartItem));
        }

        [HttpPost("clear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearCart()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid user ID in token.");

            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null)
                return NotFound("Patient not found.");

            int patientId = patient.PatientId;

            var cartItems = await _cartRepository.GetAllAsync(c => c.PatientId == patientId);
            if (cartItems == null || !cartItems.Any())
                return NotFound("Your cart is already empty.");

            await _cartRepository.RemoveRangeAsync(cartItems);
            await _cartRepository.SaveAsync();

            return Ok(new { message = "Cart has been cleared successfully." });
        }

    }
}
