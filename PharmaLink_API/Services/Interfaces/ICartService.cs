using PharmaLink_API.Models.DTO.CartDTO;

namespace PharmaLink_API.Services.Interfaces
{
    public interface ICartService
    {
        /// <summary>
        /// Retrieves a summary of the cart for the specified account, including cart items and order summary.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <returns>A summary DTO of the cart, or null if not found.</returns>
        Task<CartItemSummaryDTO?> GetCartSummaryAsync(string accountId);

        /// <summary>
        /// Adds an item to the cart for the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="cartItemDto">The DTO containing item details to add.</param>
        /// <returns>A tuple containing the added cart item and the total count of items in the cart.</returns>
        Task<(CartItemResponseDTO cartItem, int totalCount)> AddToCartAsync(string accountId, AddToCartDTO cartItemDto);

        /// <summary>
        /// Removes an item from the cart for the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="dto">The DTO specifying which item to remove.</param>
        Task RemoveItemFromCartAsync(string accountId, CartUpdateDTO dto);

        /// <summary>
        /// Increments the quantity of a specific cart item for the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="dto">The DTO specifying which item to increment.</param>
        /// <returns>The updated cart item DTO.</returns>
        Task<CartItemResponseDTO> IncrementCartItemAsync(string accountId, CartUpdateDTO dto);

        /// <summary>
        /// Decrements the quantity of a specific cart item for the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="dto">The DTO specifying which item to decrement.</param>
        /// <returns>The updated cart item DTO.</returns>
        Task<CartItemResponseDTO> DecrementCartItemAsync(string accountId, CartUpdateDTO dto);

        /// <summary>
        /// Clears all items from the cart for the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        Task ClearCartAsync(string accountId);
    }
}
