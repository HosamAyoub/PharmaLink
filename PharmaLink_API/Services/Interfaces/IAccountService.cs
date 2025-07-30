using Microsoft.AspNetCore.Identity;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.LoginAccoutDTO;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;

namespace PharmaLink_API.Services.Interfaces
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterAsync(RegisterAccountDTO account);
        Task<IResult> LoginAsync(LoginDTO loginInfo);
        //Task<IResult> ChangePasswordAsync(string email, string oldPassword, string newPassword);
        //Task<IResult> UpdateAccountAsync(RegisterAccountDTO account);
        //Task<IResult> DeleteAccountAsync(string email);
        //Task<IResult> GetAccountDetailsAsync(string email);
        //Task<IResult> GetAllAccountsAsync();
    }
}
