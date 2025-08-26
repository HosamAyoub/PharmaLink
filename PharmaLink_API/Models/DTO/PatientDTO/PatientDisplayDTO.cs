using PharmaLink_API.Core.Enums;

namespace PharmaLink_API.Models.DTO.PatientDTO
{
    public class PatientDisplayDTO
    {
        public int PatientId { get; set; }
        public string userId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Country { get; set; }
        public string? Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int OrderCount { get; set; } = 0;
        public User_Status? Status { get; set; }

    }
}
