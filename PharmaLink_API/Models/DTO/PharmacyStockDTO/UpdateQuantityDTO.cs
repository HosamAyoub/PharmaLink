using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.PharmacyStockDTO
{
    public class UpdateQuantityDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Drug ID must be a positive number.")]
        public int DrugId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative.")]
        public int QuantityAvailable { get; set; }
    }
}
