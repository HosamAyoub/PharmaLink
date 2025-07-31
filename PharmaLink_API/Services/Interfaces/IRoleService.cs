using Microsoft.AspNetCore.Identity;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;
using System.Security.Principal;

namespace PharmaLink_API.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IdentityResult> assignRoleAsync(RegisterAccountDTO accountDto, Account newAccount);
    }
}
