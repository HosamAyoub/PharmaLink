using PharmaLink_API.Core.Attributes;
using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.PharmacyStockDTO
{
    public class PharmacyDTO
    {

        public string Name { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public string? PhoneNumber { get; set; }
        public double? Rate { get; set; }
        public TimeOnly? StartHour { get; set; }
        public TimeOnly? EndHour { get; set; }


    }
}
