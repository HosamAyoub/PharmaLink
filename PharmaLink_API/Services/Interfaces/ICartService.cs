using PharmaLink_API.Models.DTO.CartDTO;

namespace PharmaLink_API.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartItemSummaryDTO?> GetCartSummaryAsync(string accountId);
        Task<(CartItemResponseDTO cartItem, int totalCount)> AddToCartAsync(string accountId, AddToCartDTO cartItemDto);
        Task RemoveItemFromCartAsync(string accountId, CartUpdateDTO dto);
        Task<CartItemResponseDTO> IncrementCartItemAsync(string accountId, CartUpdateDTO dto);
        Task<CartItemResponseDTO> DecrementCartItemAsync(string accountId, CartUpdateDTO dto);
        Task ClearCartAsync(string accountId);
    }
}
