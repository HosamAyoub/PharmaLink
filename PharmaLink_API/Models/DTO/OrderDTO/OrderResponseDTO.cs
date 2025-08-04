namespace PharmaLink_API.Models.DTO.OrderDTO
{
    public class OrderResponseDTO
    {
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public string Message { get; set; }
    }
}
