namespace PharmaLink_API.Models.DTO.PharmacyStockDTO
{
    public class PharmacyProductDetailsDTO
    {
        public int DrugId { get; set; }
        public string? DrugName { get; set; }
        public string? DrugCategory { get; set; }
        public string? DrugActiveIngredient { get; set; }
        public string? DrugDescription { get; set; }
        public string? DrugImageUrl { get; set; }
        public int PharmacyId { get; set; }
        public string? PharmacyName { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
        public Product_Status Status { get; set; } 
    }
}
