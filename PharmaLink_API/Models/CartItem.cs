namespace PharmaLink_API.Models
{
    public class CartItem
    {
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int DrugId { get; set; }
        public int PharmacyId { get; set; }
        public PharmacyStock? PharmacyStocks { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
