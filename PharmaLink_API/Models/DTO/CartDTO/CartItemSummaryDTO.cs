namespace PharmaLink_API.Models.DTO.CartDTO
{
    public class CartItemSummaryDTO
    {
        public IEnumerable<CartItemDetailsDTO> CartItems { get; set; }
        public OrderSummaryDTO OrderSummary { get; set; }
    }
}
