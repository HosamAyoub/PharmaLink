using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.CartDTO
{
    public class AddMultipleItemsToCartDTO
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one item must be provided")]
        public List<AddToCartDTO> Items { get; set; } = new List<AddToCartDTO>();
    }
}