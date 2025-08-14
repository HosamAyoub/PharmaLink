using PharmaLink_API.Core.Enums;

namespace PharmaLink_API.Models.DTO.PharmacyDTO
{
    public class PharmacyDisplayDTO
    {
        public int PharmacyID { get; set; }
        public string Name { get; set; }
        public string? OwnerName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? ImgUrl { get; set; }
        public string? DocURL { get; set; }
        public double? Rate { get; set; }
        public Pharmacy_Status Status { get; set; }
        public DateTime JoinedDate { get; set; }
        public TimeOnly? StartHour { get; set; }
        public TimeOnly? EndHour { get; set; }
    }
}
