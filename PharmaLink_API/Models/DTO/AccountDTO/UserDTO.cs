using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.AccountDTO
{
    public class UserDTO
    {
        [Required]
        public Gender Gender { get; set; }
        [Required]
        public DateOnly DateOfBirth { get; set; }
        [Required]
        public string Country { get; set; }
        // we will convert it to langitutde and latitude
        public string? Address { get; set; }
        public string? UserDiseases { get; set; }
        public string? UserDrugs { get; set; }
        public string AccountId { get; set; }
    }
}
