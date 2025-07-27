using Microsoft.AspNetCore.Identity;

namespace PharmaLink_API.Repository.Interfaces
{
    public interface IRoleRepository
    {
        /// <summary>
        /// Creates a new role in the system.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        /// <returns>A task representing the asynchronous operation, with a result indicating success or failure.</returns>
        Task<IdentityResult> CreateRoleAsync(string roleName);
        /// <summary>
        /// Deletes an existing role from the system by id.
        /// </summary>
        /// <param name="roleId">The ID of the role to delete.</param>
        /// <returns>A task representing the asynchronous operation, with a result indicating success or failure.</returns>
        Task<IdentityResult> DeleteRoleByIdAsync(string roleId);
        /// <summary>
        /// Deletes and existing role from the system by its name.
        /// </summary>
        /// <param name="roleName">The ID of the role to delete.</param>
        /// <returns>A task representing the asynchronous operation, with a result indicating success or failure.</returns>
        Task<IdentityResult> DeleteRoleByNameAsync(string roleName);
        /// <summary>
        /// Gets all roles in the system.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of roles.</returns>
        Task<IList<IdentityRole>> GetAllRolesAsync();
    }
}
