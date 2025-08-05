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
        public string? PatientDiseases { get; set; }
        public string? PatientDrugs { get; set; }
    }
}
