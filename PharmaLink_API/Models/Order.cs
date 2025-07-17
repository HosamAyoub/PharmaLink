namespace PharmaLink_API.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public string Address { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string Name { get; set; }
        public DateTime OrderDate { get; set; }

        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }

        //Pharmacy-Order relationship (one to many)
        public int PharmacyId { get; set; }
        public Pharmacy? Pharmacy { get; set; }

        //Patient-Order relationship (one to many)
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        //OrderDetail-Order relationship (many to one)
        public ICollection<OrderDetail>? OrderDetails { get; set; }

        //Order-CartItem relationship (one to many)
        //public ICollection<CartItem>? CartItems { get; set; }
    }
}
