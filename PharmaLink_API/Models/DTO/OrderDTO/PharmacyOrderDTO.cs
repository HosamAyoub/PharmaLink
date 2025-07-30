namespace PharmaLink_API.Models.DTO.OrderDTO
{
    public class PharmacyOrderDTO
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public string PaymentStatus { get; set; }
        public List<OrderItemDTO> OrderDetails { get; set; }
    }
}
