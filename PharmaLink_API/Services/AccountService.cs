using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PharmaLink_API.Core.Constants;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace PharmaLink_API.Services
{
    public class AccountService : IAccountService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAccountRepository _accountRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly UserManager<Account> _userManager;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AccountService(UserManager<Account> userManager, IMapper mapper, IConfiguration configuration, RoleManager<IdentityRole> roleManager, IAccountRepository accountRepository, IPatientRepository patientRepository, IPharmacyRepository pharmacyRepository)
        {
            _userManager = userManager;
            _mapper = mapper;
            _config = configuration;
            _roleManager = roleManager;
            _accountRepository = accountRepository;
            _patientRepository = patientRepository;
            _pharmacyRepository = pharmacyRepository;
        }
        public async Task<IdentityResult> RegisterAsync(RegisterAccountDTO account)
        {
            if (account == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User cannot be null" });
            }

            var transaction = await _accountRepository.StartTransactionAsync();

            Account newAccount = _mapper.Map<Account>(account);
            IdentityResult result = await _userManager.CreateAsync(newAccount, account.PasswordHash);

            if (!result.Succeeded)
            {
                return result;
            }

            if (account.Patient != null)
            {
                newAccount.Patient = _mapper.Map<Patient>(account.Patient);
                newAccount.Patient.AccountId = newAccount.Id;
                await _patientRepository.CreateAsync(newAccount.Patient);
            }
            else if (account.Pharmacy != null)
            {
                newAccount.Pharmacy = _mapper.Map<Pharmacy>(account.Pharmacy);
                newAccount.Pharmacy.AccountId = newAccount.Id;
                await _pharmacyRepository.CreateAsync(newAccount.Pharmacy);
            }



            // Determine user role based on account type
            UserRole userRole = account.Patient != null ? UserRole.Patient :
                               account.Pharmacy != null ? UserRole.Pharmacy :
                               UserRole.Admin;

            string roleName = userRole.ToRoleString();

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await transaction.RollbackAsync();
                return IdentityResult.Failed(new IdentityError { Description = $"Role '{roleName}' does not exist." });
            }

            var roleResult = await _userManager.AddToRoleAsync(newAccount, roleName);

            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return IdentityResult.Failed(roleResult.Errors.ToArray());
            }

            await _accountRepository.SaveAsync();
            await _accountRepository.EndTransactionAsync();

            return result;

        }

        public async Task<IResult> LoginAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return Results.BadRequest("Email or password cannot be empty.");
            }
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Results.Unauthorized();
            }
            var authenticated = await _userManager.CheckPasswordAsync(user, password);
            if (!authenticated)
            {
                return Results.Unauthorized();
            }




            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),   // JWT ID
            };

            // Add pharmacy-specific claims if user is a pharmacy
            var pharmacy = await _pharmacyRepository.GetAsync(p => p.AccountId == user.Id, includeProperties: p => p.Account);
            if (pharmacy != null)
            {
                claims.Add(new Claim(CustomClaimTypes.PharmacyId, pharmacy.PharmacyID.ToString()));
            }


            // Add roles
            foreach (var role in await _userManager.GetRolesAsync(user))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            SigningCredentials signInCred = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"])),
                SecurityAlgorithms.HmacSha256
            );

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(_config["JWT:ExpirationMinutes"])),
                signingCredentials: signInCred
            );

            return Results.Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                expiration = jwtSecurityToken.ValidTo,
                userName = user.UserName,
            });
        }
    }
}
