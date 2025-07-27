using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.PharmacyStockDTO
{
 
    public class pharmacyProductDTO
    {
        public int DrugId { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
    }

    public class UpdatePriceOnlyDTO
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }
    }

    public class IncreaseQuantityDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity to increase must be a positive number.")]
        public int Quantity { get; set; }
    }

    public class DecreaseQuantityDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity to decrease must be a positive number.")]
        public int Quantity { get; set; }
    }
}
