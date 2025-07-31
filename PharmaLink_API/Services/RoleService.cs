using Microsoft.AspNetCore.Identity;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;
using PharmaLink_API.Services.Interfaces;

namespace PharmaLink_API.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<Account> _userManager;
        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<Account> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task<IdentityResult> assignRoleAsync(RegisterAccountDTO accountDto, Account newAccount)
        {
            UserRole userRole = accountDto.Patient != null ? UserRole.Patient :
                   accountDto.Pharmacy != null ? UserRole.Pharmacy :
                   UserRole.Admin;
            string roleName = userRole.ToRoleString();

            return await _userManager.AddToRoleAsync(newAccount, roleName); ;
        }
    }
}
