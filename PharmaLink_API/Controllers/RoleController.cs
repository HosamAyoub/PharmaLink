using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Repository.IRepository;

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
            if(ModelState.IsValid)
            {
                IdentityResult result = await _roleRepository.CreateRoleAsync(roleName);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
            }
            return Ok(new { Message = $"Role '{roleName}' created successfully." });
        }
        [HttpDelete("DeleteRole")]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await _roleRepository.DeleteRoleAsync(roleId);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
            }
            return Ok(new { Message = $"Role with ID '{roleId}' deleted successfully." });
        }
        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleRepository.GetAllRolesAsync();
            return Ok(roles);
        }
    }
}
