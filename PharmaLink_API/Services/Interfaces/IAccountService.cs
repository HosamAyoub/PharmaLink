using Microsoft.AspNetCore.Identity;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.LoginAccoutDTO;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;

namespace PharmaLink_API.Services.Interfaces
{
    /// <summary>
    /// Interface for account-related service operations.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="account">The registration data for the new user.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        Task<IdentityResult> RegisterAsync(RegisterAccountDTO account);

        /// <summary>
        /// Authenticates a user and returns a JWT token if successful.
        /// </summary>
        /// <param name="loginInfo">The login credentials.</param>
        /// <returns>IResult with token or error message.</returns>
        Task<IResult> LoginAsync(LoginDTO loginInfo);
        Task<IResult> VerifyTokenAsync(string token);
        //Task<IResult> ChangePasswordAsync(string email, string oldPassword, string newPassword);
        //Task<IResult> UpdateAccountAsync(RegisterAccountDTO account);
        //Task<IResult> DeleteAccountAsync(string email);
        //Task<IResult> GetAccountDetailsAsync(string email);
        //Task<IResult> GetAllAccountsAsync();

        Task<bool?> EmailIsConfirmedAsync(string userId, string token);
    }
}
