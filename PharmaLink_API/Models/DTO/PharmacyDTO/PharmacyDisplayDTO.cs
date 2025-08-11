namespace PharmaLink_API.Models.DTO.PharmacyDTO
{
    public class PharmacyDisplayDTO
    {
        public int PharmacyID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? ImgUrl { get; set; }
        public double? Rate { get; set; }
        public TimeOnly? StartHour { get; set; }
        public TimeOnly? EndHour { get; set; }
    }
}
