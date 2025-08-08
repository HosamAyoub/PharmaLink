namespace PharmaLink_API.Models.DTO.PharmacyDTO
{
    public class PharmacyUpdateDTO
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public TimeOnly? StartHour { get; set; }
        public TimeOnly? EndHour { get; set; }
        public IFormFile? Photo { get; set; }
    }
}
