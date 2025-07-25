namespace PharmaLink_API.Models
{
    public class PharmacyProduct
    {
        public int DrugId { get; set; }
        public required Drug Drug { get; set; }

        public int PharmacyId { get; set; }
        public required Pharmacy Pharmacy { get; set; }

        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }

        //User-PharmacyStock(Cart) relationship (many to many)
        public ICollection<CartItem>? CartItems { get; set; }

        //OrderDetail-PharmacyStock relationship (many to one)
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
