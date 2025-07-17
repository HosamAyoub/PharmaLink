using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Repository.IRepository;

namespace PharmaLink_API.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public RoleRepository(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        public async Task<IdentityResult> CreateRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role name cannot be null or empty." });
            }

            // Check if the role already exists
            var existingRole = await _roleManager.FindByNameAsync(roleName);
            if (existingRole != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Role '{roleName}' already exists." });
            }

            var role = new IdentityRole(roleName);
            return await _roleManager.CreateAsync(role);
        }

        public async Task<IdentityResult> DeleteRoleAsync(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role ID cannot be null or empty." });
            }

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });
            }

            return await _roleManager.DeleteAsync(role);
        }

        public async Task<IList<IdentityRole>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.ToListAsync();
        }
    }
}
