namespace PharmaLink_API.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }

        //OrderDetail-Order relationship (many to one)
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        //OrderDetail-PharmacyStock relationship (many to one)
        public int DrugId { get; set; }
        public int PharmacyId { get; set; }
        public PharmacyStock? PharmacyStock { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
        // Additional properties can be added as needed
    }
}
