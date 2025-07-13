namespace PharmaLink_API.Models.DTO.CartDTO
{
    public class CartItemSummaryDTO
    {
        public IEnumerable<AddToCartDTO> cartItems { get; set; }
        public Order order { get; set; }
    }
}
