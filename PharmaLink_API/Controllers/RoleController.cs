using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Models.DTO.RolesDTO;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleService _roleService;
        private readonly IPharmacyRepository _pharmacyRepository;

        public RoleController(IRoleRepository roleRepository, IRoleService roleService, IPharmacyRepository pharmacyRepository)
        {
            _roleService = roleService;
            _roleRepository = roleRepository;
            _pharmacyRepository = pharmacyRepository;
        }

        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
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
        [HttpPut("changeRole")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(string userId, string newRoleName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(newRoleName))
            {
                return BadRequest(new { Errors = "User ID and Role Name cannot be null or empty." });
            }
            var user = await _roleService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { Error = $"User with ID '{userId}' not found." });
            }
            IdentityResult result = await _roleService.ChangeUserRoleAsync(user, newRoleName);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok(new { Message = $"User '{user.UserName}' role changed to '{newRoleName}' successfully." });
        }

        [HttpPut("changeRoleByPharmacy")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRoleByPharmacy([FromBody] ChangeRoleRequest request)
        {
            if (request.PharmacyId <= 0 || string.IsNullOrWhiteSpace(request.NewRoleName))
            {
                return BadRequest(new { Errors = "Pharmacy ID and Role Name cannot be null or empty." });
            }

            // Get the pharmacy by ID
            var pharmacy = await _pharmacyRepository.GetAsync(p => p.PharmacyID == request.PharmacyId, true, x => x.Account);
            if (pharmacy == null || string.IsNullOrEmpty(pharmacy.AccountId))
            {
                return NotFound(new { Error = $"Pharmacy with ID '{request.PharmacyId}' not found or has no associated user." });
            }

            // Get the user by AccountId
            var user = await _roleService.GetUserByIdAsync(pharmacy.AccountId);
            if (user == null)
            {
                return NotFound(new { Error = $"User with ID '{pharmacy.AccountId}' not found." });
            }

            IdentityResult result = await _roleService.ChangeUserRoleAsync(user, request.NewRoleName);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok(new { Message = $"User '{user.UserName}' role changed to '{request.NewRoleName}' successfully." });
        }
    }
}