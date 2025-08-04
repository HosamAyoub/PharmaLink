using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;

namespace PharmaLink_API.Repository.Interfaces
{
    /// <summary>
    /// Interface for account-specific repository operations.
    /// </summary>
    public interface IAccountRepository : IRepository<Account>
    {
        /// <summary>
        /// Starts a new database transaction for account operations.
        /// </summary>
        /// <returns>IDbContextTransaction object for transaction control.</returns>
        Task<IDbContextTransaction> StartTransactionAsync();

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        /// <returns>A task representing the commit operation.</returns>
        Task EndTransactionAsync();

        // Additional methods specific to account management can be added here
        // For example, methods for retrieving accounts, updating accounts, etc.
        // Task<Account> GetAccountByEmailAsync(string email);
        // Task<IEnumerable<Account>> GetAllAccountsAsync();
        // Task<IdentityResult> UpdateAccountAsync(Account account);
        // Task<IdentityResult> DeleteAccountAsync(string email);
    }
}
