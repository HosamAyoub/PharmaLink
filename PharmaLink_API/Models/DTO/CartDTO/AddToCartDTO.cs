namespace PharmaLink_API.Models.DTO.CartDTO
{
    public class AddToCartDTO
    {
        public int DrugId { get; set; }
        public int PharmacyId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int PatientId { get; set; } 
    }
}
