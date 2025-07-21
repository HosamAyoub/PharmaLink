using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models
{
    public class Pharmacy
    {
        public int PharmacyID { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        [MaxLength(150, ErrorMessage = "Address cannot exceed 150 characters.")]
        public string Address { get; set; }
        //public string Longitude { get; set; }
        //public string Latitude { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }
        [Range(0,5, ErrorMessage = "Rate must be between 0 and 5.")]
        public double? Rate { get; set; }
        [DataType(DataType.Time)]
        [Range(0, 24, ErrorMessage = "Start hour must be between 0 and 24.")]
        public TimeOnly? StartHour { get; set; }
        [DataType(DataType.Time)]
        [Range(0, 24, ErrorMessage = "Start hour must be between 0 and 24.")]
        public TimeOnly? EndHour { get; set; }

        //Pharmacy-Account relationship (one to one)
        public string AccountId { get; set; }
        public Account Account { get; set; }

        //Pharmacy-Drug relationship (many to many)
        public ICollection<PharmacyStock>? PharmacyStocks { get; set; }

        //Pharmacy-Order relationship (one to many)
        public ICollection<Order>? Orders { get; set; }
    }
}
