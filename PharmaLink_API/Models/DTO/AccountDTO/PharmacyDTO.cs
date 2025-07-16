using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.AccountDTO
{
    public class PharmacyDTO
    {
        [Required]
        public string Country { get; set; }
        [Required]
        public string Address { get; set; }
        
        public TimeOnly? StartHour { get; set; }
        public TimeOnly? EndHour { get; set; }
        public string AccountId { get; set; }
    }
}
