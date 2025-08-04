using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Repository.Interfaces;

namespace PharmaLink_API.Repository
{
    // Repository for managing roles and related operations
    public class RoleRepository : IRoleRepository
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        // Inject the RoleManager for IdentityRole
        public RoleRepository(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        /// <summary>
        /// Creates a new role if it does not already exist.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
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

            // Create and persist the new role
            var role = new IdentityRole(roleName);
            return await _roleManager.CreateAsync(role);
        }

        /// <summary>
        /// Deletes a role by its unique identifier.
        /// </summary>
        /// <param name="roleId">The ID of the role to delete.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        public async Task<IdentityResult> DeleteRoleByIdAsync(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role ID cannot be null or empty." });
            }

            // Find the role by ID
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });
            }

            // Delete the role
            return await _roleManager.DeleteAsync(role);
        }

        /// <summary>
        /// Deletes a role by its name.
        /// </summary>
        /// <param name="roleName">The name of the role to delete.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        public async Task<IdentityResult> DeleteRoleByNameAsync(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role Name cannot be null or empty." });
            }

            // Find the role by name
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });
            }

            // Delete the role
            return await _roleManager.DeleteAsync(role);
        }

        /// <summary>
        /// Retrieves all roles in the system.
        /// </summary>
        /// <returns>List of IdentityRole objects.</returns>
        public async Task<IList<IdentityRole>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.ToListAsync();
        }
    }
}
