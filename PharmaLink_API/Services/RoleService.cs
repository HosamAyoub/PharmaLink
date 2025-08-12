using Microsoft.AspNetCore.Identity;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;
using PharmaLink_API.Services.Interfaces;

namespace PharmaLink_API.Services
{
    // Service for handling user role assignment logic
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<Account> _userManager;

        // Inject RoleManager and UserManager for role operations
        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<Account> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Assigns a role to a new account based on the registration DTO.
        /// </summary>
        /// <param name="accountDto">The registration data.</param>
        /// <param name="newAccount">The newly created account.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        public async Task<IdentityResult> assignRoleAsync(RegisterAccountDTO accountDto, Account newAccount)
        {
            // Determine the user role based on the registration data
            UserRole userRole = accountDto.Patient != null ? UserRole.Patient :
                   accountDto.Pharmacy != null ? UserRole.pending :
                   UserRole.Admin;
            string roleName = userRole.ToRoleString();

            // Assign the role to the user
            return await _userManager.AddToRoleAsync(newAccount, roleName);
        }

        /// <summary>
        /// Retrieves a user by their unique ID.
        /// </summary>
        public async Task<Account?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        /// <summary>
        /// Changes the user's role to the specified new role.
        /// Removes all current roles and adds the new one.
        /// </summary>
        public async Task<IdentityResult> ChangeUserRoleAsync(Account user, string newRoleName)
        {
            // Get all current roles for the user
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove user from all current roles
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return removeResult;
            }

            // Add user to the new role
            var addResult = await _userManager.AddToRoleAsync(user, newRoleName);
            return addResult;
        }
    }
}
