using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.RolesDTO
{
    public class RoleDTO : IdentityRole
    {
        [Required]
        public string Name { get; set; }
    }
}
