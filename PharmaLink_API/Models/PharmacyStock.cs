namespace PharmaLink_API.Models
{
    public class PharmacyStock
    {
        public int DrugId { get; set; }
        public Drug? Drug { get; set; }

        public int PharmacyId { get; set; }
        public Pharmacy? Pharmacy { get; set; }

        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }

        //User-PharmacyStock(Cart) relationship (many to many)
        public ICollection<CartItem>? CartItems { get; set; }
    }
}
