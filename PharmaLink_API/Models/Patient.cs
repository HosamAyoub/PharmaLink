using System.ComponentModel.DataAnnotations;
using PharmaLink_API.Core.Enums;

namespace PharmaLink_API.Models
{
    public class Patient
    {
        public int PatientId { get; set; }
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Country { get; set; }
        public string? Address { get; set; }
        public string? MedicalHistory { get; set; }
        public string? Medications { get; set; }
        public string? Allergies { get; set; }

        //Patient-Account relationship (one to one)
        public string AccountId { get; set; }
        public Account Account { get; set; }

        //Patient-Order relationship (one to many)
        public ICollection<Order>? Orders { get; set; }

        //Patient-PharmacyStock(Cart) relationship (many to many)
        public ICollection<CartItem>? CartItems { get; set; }

        //Patient-Favorites relationship (one to many)
        public ICollection<PatientFavoriteDrug>? PatientFavorites { get; set; }
    }
}
