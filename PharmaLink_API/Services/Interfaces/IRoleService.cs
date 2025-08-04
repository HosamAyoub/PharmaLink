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
    }
}
