namespace PharmaLink_API.Models.DTO.CartDTO
{
    public class OrderItemDTO
    {
        public int DrugId { get; set; }
        public int PharmacyId { get; set; }
        public int Quantity { get; set; }
    }
}
