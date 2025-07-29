using AutoMapper;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.CartDTO;
using PharmaLink_API.Repository;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;

namespace PharmaLink_API.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IPharmacyStockRepository _pharmacyStockRepository;
        private readonly IMapper _mapper;
        public CartService(ICartRepository cartRepository, IPatientRepository patientRepository, IPharmacyStockRepository pharmacyStockRepository,IMapper mapper)
        {
            _cartRepository = cartRepository;
            _patientRepository = patientRepository;
            _pharmacyStockRepository = pharmacyStockRepository;
            _mapper = mapper;
        }

        // Retrieves a full summary of the current user's cart, including items and order summary
        public async Task<CartItemSummaryDTO?> GetCartSummaryAsync(string accountId)
        {
            // Get patient based on their account ID (from token)
            var patient = await GetRequiredPatientAsync(accountId);

            // Get all cart items for the patient, including related drug and pharmacy info
            var cartItems = await GetCartItemsAsync(patient.PatientId);
            if (cartItems == null || !cartItems.Any())
                return null;

            // Map each cart item to a DTO
            var cartItemDtos = _mapper.Map<List<CartItemDetailsDTO>>(cartItems);
            // Calculate subtotal: sum of (unit price × quantity)
            var subtotal = cartItemDtos.Sum(x => x.UnitPrice * x.Quantity);
            // Set a fixed delivery fee
            var deliveryFee = 4.99m;

            // Map patient info to the order summary DTO
            var orderDto = _mapper.Map<OrderSummaryDTO>(patient);
            orderDto.Subtotal = subtotal;
            orderDto.DeliveryFee = deliveryFee;

            // Return the complete cart summary
            return new CartItemSummaryDTO
            {
                CartItems = cartItemDtos,
                OrderSummary = orderDto
            };
        }

        public async Task<(CartItemResponseDTO cartItem, int totalCount)> AddToCartAsync(string accountId, AddToCartDTO cartItemDto)
        {
            // Get the patient based on account ID
            var patient = await GetRequiredPatientAsync(accountId);
            int patientId = patient.PatientId;

            // Check if the drug is available in the selected pharmacy
            var stockExists = await _pharmacyStockRepository.GetAsync(
                s => s.DrugId == cartItemDto.DrugId && s.PharmacyId == cartItemDto.PharmacyId);
            if (stockExists == null)
                throw new InvalidOperationException("Drug is not available in selected pharmacy.");

            decimal stockPrice = stockExists.Price;

            // Get existing cart item if it already exists
            var existingCartItem = await _cartRepository.GetAsync(
                u => u.PatientId == patientId && u.DrugId == cartItemDto.DrugId && u.PharmacyId == cartItemDto.PharmacyId);

            // Check that all items in cart are from the same pharmacy
            await EnsureSamePharmacyOnlyAsync(patientId, cartItemDto.PharmacyId);

            // Add new item or increment quantity if it already exists
            var finalCartItem = await AddOrUpdateCartItemAsync(existingCartItem, cartItemDto, patientId, stockExists.Price);

            await _cartRepository.SaveAsync();

            var responseDTO = _mapper.Map<CartItemResponseDTO>(finalCartItem);
            var totalCount = await GetUpdatedCartCountAsync(patientId);

            return (responseDTO, totalCount);
        }

        public async Task RemoveItemFromCartAsync(string accountId, CartUpdateDTO dto)
        {
            // Get the patient linked to the account ID
            var patient = await GetRequiredPatientAsync(accountId);
            int patientId = patient.PatientId;

            // Retrieve the cart item to be removed
            var cartItem = await GetRequiredCartItemAsync(patient.PatientId, dto);

            await _cartRepository.RemoveAsync(cartItem);
            await _cartRepository.SaveAsync();
        }

        public async Task<CartItemResponseDTO> IncrementCartItemAsync(string accountId, CartUpdateDTO dto)
        {
            if (dto == null || dto.DrugId <= 0 || dto.PharmacyId <= 0)
                throw new ArgumentException("Invalid cart update request");

            var patient = await GetRequiredPatientAsync(accountId);
            int patientId = patient.PatientId;

            var cartItem = await GetRequiredCartItemAsync(patient.PatientId, dto);

            _cartRepository.IncrementCount(cartItem, 1);
            await _cartRepository.SaveAsync();

            return _mapper.Map<CartItemResponseDTO>(cartItem);
        }

        public async Task<CartItemResponseDTO> DecrementCartItemAsync(string accountId, CartUpdateDTO dto)
        {
            if (dto == null || dto.DrugId <= 0 || dto.PharmacyId <= 0)
                throw new ArgumentException("Invalid cart update request");

            var patient = await GetRequiredPatientAsync(accountId);
            int patientId = patient.PatientId;

            var cartItem = await GetRequiredCartItemAsync(patient.PatientId, dto);

            if (cartItem.Quantity <= 1)
            {
                await _cartRepository.RemoveAsync(cartItem);
                await _cartRepository.SaveAsync();
                return null;
            }

            _cartRepository.DecrementCount(cartItem, 1);
            await _cartRepository.SaveAsync();

            return _mapper.Map<CartItemResponseDTO>(cartItem);
        }

        public async Task ClearCartAsync(string accountId)
        {
            var patient = await GetRequiredPatientAsync(accountId);
            int patientId = patient.PatientId;

            var cartItems = await GetCartItemsAsync(patient.PatientId);
            if (cartItems == null || !cartItems.Any())
                throw new InvalidOperationException("Your cart is already empty.");

            await _cartRepository.RemoveRangeAsync(cartItems);
            await _cartRepository.SaveAsync();
        }


        //** Helpers for internal logic **//

        // Retrieves all cart items for a specific patient, including related drug and pharmacy
        private async Task<List<CartItem>> GetCartItemsAsync(int patientId)
        {
            return await _cartRepository.GetAllAsync(
                u => u.PatientId == patientId,
                x => x.PharmacyProduct!.Drug,
                x => x.PharmacyProduct!.Pharmacy
            );
        }

        // Check that all items in cart are from the same pharmacy
        private async Task EnsureSamePharmacyOnlyAsync(int patientId, int newPharmacyId)
        {
            var cartList = await _cartRepository.GetAllAsync(c => c.PatientId == patientId);
            if (cartList.Any() && cartList.First().PharmacyId != newPharmacyId)
                throw new InvalidOperationException("You can only add drugs from one pharmacy at a time.");
        }

        private async Task<CartItem> AddOrUpdateCartItemAsync(CartItem? existingCartItem, AddToCartDTO dto, int patientId, decimal price)
        {
            if (existingCartItem == null)
            {
                var newItem = _mapper.Map<CartItem>(dto);
                newItem.PatientId = patientId;
                newItem.Price = price;
                await _cartRepository.CreateAsync(newItem);
                return newItem;
            }

            _cartRepository.IncrementCount(existingCartItem, dto.Quantity);
            return existingCartItem;
        }

        private async Task<int> GetUpdatedCartCountAsync(int patientId)
        {
            var cartItems = await _cartRepository.GetAllAsync(c => c.PatientId == patientId);
            return cartItems.Count;
        }

        private async Task<Patient> GetRequiredPatientAsync(string accountId)
        {
            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId, true, x => x.Account);
            if (patient == null)
                throw new ArgumentException("Patient not found");
            return patient;
        }

        private async Task<CartItem> GetRequiredCartItemAsync(int patientId, CartUpdateDTO dto)
        {
            var cartItem = await _cartRepository.GetAsync(u =>
                u.PatientId == patientId &&
                u.DrugId == dto.DrugId &&
                u.PharmacyId == dto.PharmacyId);

            if (cartItem == null)
                throw new KeyNotFoundException("Cart item not found");

            return cartItem;
        }
    }
}
