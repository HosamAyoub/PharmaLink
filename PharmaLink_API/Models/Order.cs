namespace PharmaLink_API.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public string Address { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime OrderDate { get; set; }

        //Pharmacy-Order relationship (one to many)
        public int PharmacyId { get; set; }
        public Pharmacy? Pharmacy { get; set; }

        //User-Order relationship (one to many)
        public int UserId { get; set; }
        public User? User { get; set; }

        //Order-CartItem relationship (one to many)
        public ICollection<CartItem>? CartItems { get; set; }
    }
}
