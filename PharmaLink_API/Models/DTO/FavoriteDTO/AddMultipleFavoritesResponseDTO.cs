namespace PharmaLink_API.Models.DTO.FavoriteDTO
{
    public class AddMultipleFavoritesResponseDTO
    {
        public List<FavoriteDrugDTO> AddedFavorites { get; set; } = new List<FavoriteDrugDTO>();
        public List<string> Errors { get; set; } = new List<string>();
        public List<int> SkippedDrugIds { get; set; } = new List<int>(); // Already in favorites
        public int TotalRequested { get; set; }
        public int SuccessfullyAdded { get; set; }
        public int AlreadyInFavorites { get; set; }
        public int Failed { get; set; }
    }
}