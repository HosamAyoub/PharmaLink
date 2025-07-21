using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.RegisterAccountDTO
{
    public class RegisterPatientDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public Gender Gender { get; set; }
        [Required]
        public DateOnly DateOfBirth { get; set; }
        [Required]
        public string Country { get; set; }
        public string? Address { get; set; }
        public string? PatientDiseases { get; set; }
        public string? PatientDrugs { get; set; }
    }
}
