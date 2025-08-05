using PharmaLink_API.Core.Enums;

namespace PharmaLink_API.Models.DTO.PatientDTO
{
    public class PatientDTO
    {
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Country { get; set; }
        public string? Address { get; set; }
        public string? MedicalHistory { get; set; }
        public string? Medications { get; set; }
        public string? Allergies { get; set; }
    }
}
