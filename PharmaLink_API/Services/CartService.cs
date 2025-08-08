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

        /// <summary>
        /// Initializes a new instance of the CartService class.
        /// </summary>
        public CartService(ICartRepository cartRepository, IPatientRepository patientRepository, IPharmacyStockRepository pharmacyStockRepository, IMapper mapper)
        {
            _cartRepository = cartRepository;
            _patientRepository = patientRepository;
            _pharmacyStockRepository = pharmacyStockRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a summary of the cart for the specified account, including cart items and order summary.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <returns>A summary DTO of the cart, or null if not found.</returns>
        public async Task<CartItemSummaryDTO?> GetCartSummaryAsync(string accountId)
        {
            var patient = await GetRequiredPatientAsync(accountId);

            var cartItems = await GetCartItemsInternalAsync(patient.PatientId);
            if (cartItems == null || !cartItems.Any())
                return null;

            var cartItemDtos = _mapper.Map<List<CartItemDetailsDTO>>(cartItems);
            var subtotal = cartItemDtos.Sum(x => x.UnitPrice * x.Quantity);
            var deliveryFee = 4.99m;

            var orderDto = _mapper.Map<OrderSummaryDTO>(patient);
            orderDto.Subtotal = subtotal;
            orderDto.DeliveryFee = deliveryFee;

            return new CartItemSummaryDTO
            {
                CartItems = cartItemDtos,
                OrderSummary = orderDto
            };
        }

        /// <summary>
        /// Retrieves only the cart items for the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <returns>A list of cart item DTOs, or null if cart is empty.</returns>
        public async Task<List<CartItemDetailsDTO>?> GetCartItemsAsync(string accountId)
        {
            var patient = await GetRequiredPatientAsync(accountId);

            var cartItems = await GetCartItemsInternalAsync(patient.PatientId);
            if (cartItems == null || !cartItems.Any())
                return null;

            var cartItemDtos = _mapper.Map<List<CartItemDetailsDTO>>(cartItems);
            return cartItemDtos;
        }

        /// <summary>
        /// Adds an item to the cart for the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="cartItemDto">The DTO containing item details to add.</param>
        /// <returns>A tuple containing the added cart item and the total count of items in the cart.</returns>
        public async Task<(CartItemResponseDTO cartItem, int totalCount)> AddToCartAsync(string accountId, AddToCartDTO cartItemDto)
        {
            var patient = await GetRequiredPatientAsync(accountId);
            int patientId = patient.PatientId;

            var stockExists = await _pharmacyStockRepository.GetAsync(
                s => s.DrugId == cartItemDto.DrugId && s.PharmacyId == cartItemDto.PharmacyId);
            if (stockExists == null)
                throw new InvalidOperationException("Drug is not available in selected pharmacy.");

            decimal stockPrice = stockExists.Price;

            var existingCartItem = await _cartRepository.GetAsync(
                u => u.PatientId == patientId && u.DrugId == cartItemDto.DrugId && u.PharmacyId == cartItemDto.PharmacyId);

            await EnsureSamePharmacyOnlyAsync(patientId, cartItemDto.PharmacyId);

            var finalCartItem = await AddOrUpdateCartItemAsync(existingCartItem, cartItemDto, patientId, stockExists.Price);

            await _cartRepository.SaveAsync();

            var responseDTO = _mapper.Map<CartItemResponseDTO>(finalCartItem);
            var totalCount = await GetUpdatedCartCountAsync(patientId);

            return (responseDTO, totalCount);
        }

        /// <summary>
        /// Adds multiple items to the cart for the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="cartItemsDto">The DTO containing multiple item details to add.</param>
        /// <returns>A response DTO containing information about successfully added items and any errors.</returns>
        public async Task<AddMultipleItemsToCartResponseDTO> AddMultipleItemsToCartAsync(string accountId, List<AddToCartDTO> cartItems)
        {
            var response = new AddMultipleItemsToCartResponseDTO();

            if (cartItems == null || !cartItems.Any())
            {
                response.Errors.Add("No items provided to add to cart.");
                return response;
            }

            try
            {
                var patient = await GetRequiredPatientAsync(accountId);
                int patientId = patient.PatientId;

                // Check if cart has items from different pharmacy
                var existingCartItems = await _cartRepository.GetAllAsync(c => c.PatientId == patientId);
                if (existingCartItems.Any())
                {
                    var existingPharmacyId = existingCartItems.First().PharmacyId;
                    var hasDifferentPharmacy = cartItems.Any(item => item.PharmacyId != existingPharmacyId);
                    
                    if (hasDifferentPharmacy)
                    {
                        response.Errors.Add("You can only add drugs from one pharmacy at a time.");
                        return response;
                    }
                }

                // Check that all items are from the same pharmacy
                var distinctPharmacies = cartItems.Select(i => i.PharmacyId).Distinct().ToList();
                if (distinctPharmacies.Count > 1)
                {
                    response.Errors.Add("All items must be from the same pharmacy.");
                    return response;
                }

                // Process each item
                foreach (var item in cartItems)
                {
                    try
                    {
                        // Validate item
                        if (item.DrugId <= 0 || item.PharmacyId <= 0 || item.Quantity <= 0)
                        {
                            response.Errors.Add($"Invalid item data for DrugId: {item.DrugId}, PharmacyId: {item.PharmacyId}");
                            response.Failed++;
                            continue;
                        }

                        // Check if stock exists
                        var stockExists = await _pharmacyStockRepository.GetAsync(
                            s => s.DrugId == item.DrugId && s.PharmacyId == item.PharmacyId);
                        
                        if (stockExists == null)
                        {
                            response.Errors.Add($"Drug {item.DrugId} is not available in pharmacy {item.PharmacyId}.");
                            response.Failed++;
                            continue;
                        }

                        // Check if item already exists in cart
                        var existingCartItem = await _cartRepository.GetAsync(
                            u => u.PatientId == patientId && u.DrugId == item.DrugId && u.PharmacyId == item.PharmacyId);

                        CartItem finalCartItem;
                        if (existingCartItem == null)
                        {
                            // Create new cart item
                            var newItem = _mapper.Map<CartItem>(item);
                            newItem.PatientId = patientId;
                            newItem.Price = stockExists.Price;
                            await _cartRepository.CreateAndSaveAsync(newItem);
                            finalCartItem = newItem;
                        }
                        else
                        {
                            // Update existing item quantity
                            _cartRepository.IncrementCount(existingCartItem, item.Quantity);
                            finalCartItem = existingCartItem;
                        }

                        // Add to successful items
                        var responseDto = _mapper.Map<CartItemResponseDTO>(finalCartItem);
                        response.AddedItems.Add(responseDto);
                        response.SuccessfullyAdded++;
                    }
                    catch (Exception ex)
                    {
                        response.Errors.Add($"Error adding item DrugId: {item.DrugId}, PharmacyId: {item.PharmacyId}: {ex.Message}");
                        response.Failed++;
                    }
                }

                // Save all changes
                await _cartRepository.SaveAsync();

                // Get updated cart count
                response.TotalItemsInCart = await GetUpdatedCartCountAsync(patientId);

                return response;
            }
            catch (ArgumentException ex)
            {
                response.Errors.Add(ex.Message);
                return response;
            }
            catch (Exception ex)
            {
                response.Errors.Add($"An error occurred while adding items to cart: {ex.Message}");
                return response;
            }
        }

        /// <summary>
        /// Removes an item from the cart for the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="dto">The DTO specifying which item to remove.</param>
        public async Task RemoveItemFromCartAsync(string accountId, CartUpdateDTO dto)
        {
            var patient = await GetRequiredPatientAsync(accountId);
            int patientId = patient.PatientId;

            var cartItem = await GetRequiredCartItemAsync(patient.PatientId, dto);

            await _cartRepository.RemoveAsync(cartItem);
            await _cartRepository.SaveAsync();
        }

        /// <summary>
        /// Increments the quantity of a specific cart item for the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="dto">The DTO specifying which item to increment.</param>
        /// <returns>The updated cart item DTO.</returns>
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

        /// <summary>
        /// Decrements the quantity of a specific cart item for the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="dto">The DTO specifying which item to decrement.</param>
        /// <returns>The updated cart item DTO, or null if item was removed.</returns>
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

        /// <summary>
        /// Clears all items from the cart for the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        public async Task ClearCartAsync(string accountId)
        {
            var patient = await GetRequiredPatientAsync(accountId);
            int patientId = patient.PatientId;

            var cartItems = await GetCartItemsInternalAsync(patient.PatientId);
            if (cartItems == null || !cartItems.Any())
                throw new InvalidOperationException("Your cart is already empty.");

            await _cartRepository.RemoveRangeAsync(cartItems);
            await _cartRepository.SaveAsync();
        }

        //** Helpers for internal logic **//

        /// <summary>
        /// Retrieves all cart items for a given patient.
        /// </summary>
        /// <param name="patientId">The patient ID.</param>
        /// <returns>List of CartItem entities.</returns>
        private async Task<List<CartItem>> GetCartItemsInternalAsync(int patientId)
        {
            return await _cartRepository.GetAllAsync(
                u => u.PatientId == patientId,
                x => x.PharmacyProduct!.Drug,
                x => x.PharmacyProduct!.Pharmacy
            );
        }

        /// <summary>
        /// Ensures that all items in the cart are from the same pharmacy.
        /// Throws if a different pharmacy is detected.
        /// </summary>
        /// <param name="patientId">The patient ID.</param>
        /// <param name="newPharmacyId">The pharmacy ID to check.</param>
        private async Task EnsureSamePharmacyOnlyAsync(int patientId, int newPharmacyId)
        {
            var cartList = await _cartRepository.GetAllAsync(c => c.PatientId == patientId);
            if (cartList.Any() && cartList.First().PharmacyId != newPharmacyId)
                throw new InvalidOperationException("You can only add drugs from one pharmacy at a time.");
        }

        /// <summary>
        /// Adds a new cart item or updates the quantity of an existing one.
        /// </summary>
        /// <param name="existingCartItem">The existing cart item, if any.</param>
        /// <param name="dto">The DTO containing item details.</param>
        /// <param name="patientId">The patient ID.</param>
        /// <param name="price">The price of the item.</param>
        /// <returns>The added or updated CartItem entity.</returns>
        private async Task<CartItem> AddOrUpdateCartItemAsync(CartItem? existingCartItem, AddToCartDTO dto, int patientId, decimal price)
        {
            if (existingCartItem == null)
            {
                var newItem = _mapper.Map<CartItem>(dto);
                newItem.PatientId = patientId;
                newItem.Price = price;
                await _cartRepository.CreateAndSaveAsync(newItem);
                return newItem;
            }

            _cartRepository.IncrementCount(existingCartItem, dto.Quantity);
            return existingCartItem;
        }

        /// <summary>
        /// Gets the total count of cart items for a patient.
        /// </summary>
        /// <param name="patientId">The patient ID.</param>
        /// <returns>The count of cart items.</returns>
        private async Task<int> GetUpdatedCartCountAsync(int patientId)
        {
            var cartItems = await _cartRepository.GetAllAsync(c => c.PatientId == patientId);
            return cartItems.Count;
        }

        /// <summary>
        /// Retrieves the patient entity for the given account ID.
        /// Throws if not found.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <returns>The Patient entity.</returns>
        private async Task<Patient> GetRequiredPatientAsync(string accountId)
        {
            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId, true, x => x.Account);
            if (patient == null)
                throw new ArgumentException("Patient not found");
            return patient;
        }

        /// <summary>
        /// Retrieves the required cart item for a patient and update DTO.
        /// Throws if not found.
        /// </summary>
        /// <param name="patientId">The patient ID.</param>
        /// <param name="dto">The DTO specifying the cart item.</param>
        /// <returns>The CartItem entity.</returns>
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
