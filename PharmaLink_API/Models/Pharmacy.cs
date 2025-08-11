using PharmaLink_API.Core.Attributes;
using PharmaLink_API.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models
{
    public class Pharmacy
    {
        [Key]
        public int PharmacyID { get; set; }
        [Unique]
        public string Name { get; set; }
        public string Country { get; set; }
        [MaxLength(150, ErrorMessage = "Address cannot exceed 150 characters.")]
        public string Address { get; set; }
        //public string Longitude { get; set; }
        //public string Latitude { get; set; }
        [DataType(DataType.PhoneNumber)]
        [EgyptianPhoneNumber(AcceptLandlines = true)]
        public string? PhoneNumber { get; set; }
        [Range(0,5, ErrorMessage = "Rate must be between 0 and 5.")]
        public double? Rate { get; set; }
        [DataType(DataType.Time)]
        [Range(0, 24, ErrorMessage = "Start hour must be between 0 and 24.")]
        public TimeOnly? StartHour { get; set; }
        [DataType(DataType.Time)]
        [Range(0, 24, ErrorMessage = "Start hour must be between 0 and 24.")]
        public TimeOnly? EndHour { get; set; }

        public string? ImgUrl { get; set; }

        public Pharmacy_Status Status { get; set; } = Pharmacy_Status.Pending;
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        //Pharmacy-Account relationship (one to one)
        public string AccountId { get; set; }
        public Account Account { get; set; }

        //Pharmacy-Drug relationship (many to many)
        public ICollection<PharmacyProduct>? PharmacyStock { get; set; }

        //Pharmacy-Order relationship (one to many)
        public ICollection<Order>? Orders { get; set; }
    }
}
