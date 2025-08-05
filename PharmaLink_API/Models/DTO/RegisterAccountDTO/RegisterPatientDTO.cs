using System.ComponentModel.DataAnnotations;
using PharmaLink_API.Core.Enums;

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
        public string? MedicalHistory { get; set; }
        public string? Medications { get; set; }
        public string? Allergies { get; set; }
    }
}
