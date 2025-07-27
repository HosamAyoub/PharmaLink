namespace PharmaLink_API.Models.DTO.FavoriteDTO
{
    public class FavoriteDrugDTO
    {
        public int DrugId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? DrugCategory { get; set; }
    }
}
