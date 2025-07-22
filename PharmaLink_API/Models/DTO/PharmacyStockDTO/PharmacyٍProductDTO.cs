using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.PharmacyStockDTO
{
 
    public class  pharmacyProductDTO
    {
        public int DrugId { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
    }
}
