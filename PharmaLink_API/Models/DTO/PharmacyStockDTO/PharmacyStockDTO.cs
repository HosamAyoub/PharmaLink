namespace PharmaLink_API.Models.DTO.PharmacyStockDTO
{
    public class PharmacyStockDTO
    {
        public List<pharmacyProductDTO> Products { get; set; } = new List<pharmacyProductDTO>();
    }
}
