using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Models.DTO.RolesDTO
{
    public class RoleDTO : IdentityRole
    {
        [Required]
        public string Name { get; set; }
    }
    public class ChangeRoleRequest
    {
        public int PharmacyId { get; set; }
        public string NewRoleName { get; set; }
    }
}
