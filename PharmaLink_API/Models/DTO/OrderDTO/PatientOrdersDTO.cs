namespace PharmaLink_API.Models.DTO.OrderDTO
{
    public class PatientOrdersDTO
    {
        public int OrderId { get; set; }
        public string PharmacyName { get; set; }
        public string PharmacyAddress { get; set; }
        public string PharmacyPhoneNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string DeliveryAddress { get; set; }
        public List<OrderItemDTO> OrderDetails { get; set; }
    }
}
