using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.AccountDTO
{
    public class AccountDTO
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        [Compare("PasswordHash", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public UserDTO? User { get; set; }
        public PharmacyDTO? Pharmacy { get; set; }
    }
}
