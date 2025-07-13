namespace PharmaLink_API.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string MobileNumber { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public string UserDisease { get; set; }
        public string UserDrugs { get; set; }

        //User-Account relationship (one to one)
        public Guid AccountId { get; set; }
        public Account Account { get; set; }

        //User-Order relationship (one to many)
        public ICollection<Order>? Orders { get; set; }

        //User-PharmacyStock(Cart) relationship (many to many)
        public ICollection<CartItem>? CartItems { get; set; }


        //User-Favorites relationship (one to many)
        public ICollection<UserFavoriteDrug>? UserFavorites { get; set; }

    }
}
