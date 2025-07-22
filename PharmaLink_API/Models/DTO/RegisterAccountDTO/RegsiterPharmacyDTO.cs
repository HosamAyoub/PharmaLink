using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.RegisterAccountDTO
{
    public class RegsiterPharmacyDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string Address { get; set; }
        
        public TimeOnly? StartHour { get; set; }
        public TimeOnly? EndHour { get; set; }
    }
}
