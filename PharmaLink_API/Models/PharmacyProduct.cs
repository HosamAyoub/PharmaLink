namespace PharmaLink_API.Models
{
    public class PharmacyProduct
    {
        public int DrugId { get; set; }
        public  Drug? Drug { get; set; }

        public int PharmacyId { get; set; }
        public  Pharmacy? Pharmacy { get; set; }

        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }

        public Product_Status Status { get; set; }

        //User-PharmacyStock(Cart) relationship (many to many)
        public ICollection<CartItem>? CartItems { get; set; }

        //OrderDetail-PharmacyStock relationship (many to one)
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }

    public enum Product_Status
    {
        NotAvailable,
        Available
    }

}
