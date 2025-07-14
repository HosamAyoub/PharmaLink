using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.AccountDTO
{
    public class RegisterUserDTO
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        //[Required]
        //public Gender Gender { get; set; }
        //[Required]
        //public DateOnly DateOfBirth { get; set; }
        //[Required]
        //public string PhoneNumber { get; set; }
        //[Required]
        //public string Country { get; set; }
        //// we will convert it to langitutde and latitude
        //public string? Address { get; set; }
        //public string? UserDiseases { get; set; }
        //public string? UserDrugs { get; set; }
    }
    public enum Gender
    {
        Male, Femlae
    }
}
