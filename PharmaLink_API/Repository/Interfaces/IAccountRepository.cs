using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;

namespace PharmaLink_API.Repository.Interfaces
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<IDbContextTransaction> StartTransactionAsync();
        Task EndTransactionAsync();

        // Additional methods specific to account management can be added here
        // For example, methods for retrieving accounts, updating accounts, etc.
        // Task<Account> GetAccountByEmailAsync(string email);
        // Task<IEnumerable<Account>> GetAllAccountsAsync();
        // Task<IdentityResult> UpdateAccountAsync(Account account);
        // Task<IdentityResult> DeleteAccountAsync(string email);
    }
}
