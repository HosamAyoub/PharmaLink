namespace PharmaLink_API.Models.DTO.PharmacyStockDTO
{
    public class PharmaInventoryDTO
    {
        public int InStockCount { get; set; }
        public int OutOfStockCount { get; set; }

        public int LowStockCount { get; set; }
        public int TotalCount { get; set; }


    }
}
