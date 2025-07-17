using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.RegisterAccountDTO
{
    public class RegisterAccountDTO
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string DisplayName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }
        [Required]
        [Compare("PasswordHash", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public RegisterPatientDTO? Patient { get; set; }
        public RegsiterPharmacyDTO? Pharmacy { get; set; }
    }
}
