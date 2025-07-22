using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Repository.Interfaces;

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;
        public RoleController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }
        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if(string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest(new { Error = "Role Name cannot be null or empty." });
            }
            IdentityResult result = await _roleRepository.CreateRoleAsync(roleName);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok(new { Message = $"Role '{roleName}' created successfully." });
        }
        [HttpDelete("DeleteRoleById")]
        public async Task<IActionResult> DeleteRoleById(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
            {
                    return BadRequest(new { Errors = "Role ID cannot be null or empty." });
            }
            IdentityResult result = await _roleRepository.DeleteRoleByIdAsync(roleId);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok(new { Message = $"Role with ID '{roleId}' deleted successfully." });
        }
        [HttpDelete("DeleteRoleByName")]
        public async Task<IActionResult> DeleteRoleByName(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                    return BadRequest(new { Errors = "Role Name cannot be null or empty." });
            }
            IdentityResult result = await _roleRepository.DeleteRoleByNameAsync(roleName);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok(new { Message = $"Role with Name '{roleName}' deleted successfully." });
        }
        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleRepository.GetAllRolesAsync();
            return Ok(roles);
        }
    }
}
