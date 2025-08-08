namespace PharmaLink_API.Models.DTO.CartDTO
{
    public class AddMultipleItemsToCartResponseDTO
    {
        public List<CartItemResponseDTO> AddedItems { get; set; } = new List<CartItemResponseDTO>();
        public List<string> Errors { get; set; } = new List<string>();
        public int TotalItemsInCart { get; set; }
        public int SuccessfullyAdded { get; set; }
        public int Failed { get; set; }
    }
}