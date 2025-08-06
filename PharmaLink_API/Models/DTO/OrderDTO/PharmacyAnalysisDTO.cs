namespace PharmaLink_API.Models.DTO.OrderDTO
{
    public class PharmacyAnalysisDTO
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUniqueCustomers { get; set; }
        public List<MonthlyOrderStats> MonthlyStats { get; set; }
        public List<TopSellingProduct> TopSellingProducts { get; set; }
        public List<TopCustomers> TopCustomers { get; set; }
    }

    public class MonthlyOrderStats
    {
        public string MonthYear { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopSellingProduct
    {
        public int DrugId { get; set; }
        public string DrugName { get; set; } // You'll need to add this to your OrderDetail model
        public int TotalQuantity { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
    public class TopCustomers
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } 
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
