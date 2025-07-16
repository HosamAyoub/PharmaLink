using Microsoft.AspNetCore.Identity;
using PharmaLink_API.Models.DTO.AccountDTO;

namespace PharmaLink_API.Repository.IRepository
{
    public interface IAccountRepository
    {
        Task<IdentityResult> RegisterAsync(AccountDTO user);
        //Task<SignInResult> LoginAsync(string email, string password);
        //Task<bool> IsEmailUniqueAsync(string email);
        //Task<Account> GetUserByEmailAsync(string email);
        //Task<Account> GetUserByIdAsync(string id);
        //Task<IdentityResult> UpdateUserAsync(Account user);
        //Task<IdentityResult> DeleteUserAsync(string id);
        //Task<IList<string>> GetUserRolesAsync(Account user);
        //Task<IdentityResult> AddToRoleAsync(Account user, string roleName);
        //Task<IdentityResult> RemoveFromRoleAsync(Account user, string roleName);
    }
}
