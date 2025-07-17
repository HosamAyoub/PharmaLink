namespace PharmaLink_API.Models.DTO.CartDTO
{
    public class CartItemDetailsDTO
    {
        public int drugId { get; set; }
        public int PharmacyId { get; set; }
        public string DrugName { get; set; }
        public string PharmacyName { get; set; }
        public string ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
