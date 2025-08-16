using Microsoft.AspNetCore.Identity;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;
using System.Security.Principal;

namespace PharmaLink_API.Services.Interfaces
{
    /// <summary>
    /// Interface for user role assignment service.
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Assigns a role to a new account based on registration data.
        /// </summary>
        /// <param name="accountDto">The registration data.</param>
        /// <param name="newAccount">The newly created account.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        Task<IdentityResult> assignRoleAsync(RegisterAccountDTO accountDto, Account newAccount);

        /// <summary>
        /// Retrieves a user by their unique ID.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <returns>The Account object if found, otherwise null.</returns>
        Task<Account?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Changes the user's role to the specified new role.
        /// </summary>
        /// <param name="user">The user whose role is to be changed.</param>
        /// <param name="newRoleName">The new role name.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        Task<IdentityResult> ChangeUserRoleAsync(Account user, string newRoleName);
    }
}
