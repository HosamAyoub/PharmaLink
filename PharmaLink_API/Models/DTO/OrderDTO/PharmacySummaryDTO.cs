namespace PharmaLink_API.Models.DTO.OrderDTO
{
    public class PharmacySummaryDTO
    {
        public decimal allrevenue { get; set; }
        public int allDrugStock { get; set; }
        public List<PharmacySummaryData> ? PharmacySummary { get; set; }

    }

    public class PharmacySummaryData
    {
        public int PharmacyID { get; set; }
        public string Name { get; set; }
        public double? Rate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalMedicineInStock { get; set; }
    }
}
