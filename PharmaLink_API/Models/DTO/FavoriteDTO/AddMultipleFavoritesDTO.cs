using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.FavoriteDTO
{
    public class AddMultipleFavoritesDTO
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one drug ID must be provided")]
        public List<int> DrugIds { get; set; } = new List<int>();
    }
}