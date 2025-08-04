namespace PharmaLink_API.Models.DTO.PharmacyStockDTO
{
    public class PharmacyStockDTO_WithPagination
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public long TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
        public List<PharmacyProductDetailsDTO>? Items { get; set; }
    }
}
