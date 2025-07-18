namespace PharmaLink_API.Models.DTO.CartDTO
{
    public class OrderSummaryDTO
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }

        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total => Subtotal + DeliveryFee;
    }

}
