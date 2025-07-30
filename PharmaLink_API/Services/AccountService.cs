using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PharmaLink_API.Core.Constants;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.LoginAccoutDTO;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
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
        private readonly IRoleService _roleService;

        public AccountService(UserManager<Account> userManager, IMapper mapper, IConfiguration configuration, RoleManager<IdentityRole> roleManager, IAccountRepository accountRepository, IPatientRepository patientRepository, IPharmacyRepository pharmacyRepository, IRoleService roleService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _config = configuration;
            _roleManager = roleManager;
            _accountRepository = accountRepository;
            _patientRepository = patientRepository;
            _pharmacyRepository = pharmacyRepository;
            _roleService = roleService;
        }
        public async Task<IdentityResult> RegisterAsync(RegisterAccountDTO accountDto)
        {
            if (accountDto == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User cannot be null" });
            }

            var transaction = await _accountRepository.StartTransactionAsync().ConfigureAwait(false);
            try
            {
                // Pre-validate email uniqueness to fail fast
                var existingUser = await _userManager.FindByEmailAsync(accountDto.Email).ConfigureAwait(false);
                if (existingUser != null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Email is already registered" });
                }

                Account newAccount = _mapper.Map<Account>(accountDto);
                IdentityResult result = await _userManager.CreateAsync(newAccount, accountDto.PasswordHash).ConfigureAwait(false);
                // Check if the user creation was successful
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    return result;
                }

                // Create user profile based on account type
                var profileTask = CreateUserProfileAsync(newAccount, accountDto);

                // Determine user role based on account type

                var roleTask = _roleService.assignRoleAsync(accountDto, newAccount);

                // Wait for both tasks to complete
                await Task.WhenAll(profileTask, roleTask).ConfigureAwait(false);

                var roleResult = await roleTask.ConfigureAwait(false);

                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    return roleResult;
                }

                await _accountRepository.SaveAsync().ConfigureAwait(false);
                await _accountRepository.EndTransactionAsync().ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                return IdentityResult.Failed(new IdentityError { Description = $"An error occurred while registering the account: {ex.Message}" });
            }

        }

        public async Task<IResult> LoginAsync(LoginDTO loginInfo)
        {
            if (string.IsNullOrWhiteSpace(loginInfo.Email) || string.IsNullOrWhiteSpace(loginInfo.Password))
            {
                return Results.BadRequest("Email or password cannot be empty.");
            }
            try
            {
                var user = await _userManager.FindByEmailAsync(loginInfo.Email).ConfigureAwait(false);
                if (user == null)
                {
                    return Results.Unauthorized();
                }
                var authenticated = await _userManager.CheckPasswordAsync(user, loginInfo.Password).ConfigureAwait(false);
                if (!authenticated)
                {
                    return Results.Unauthorized();
                }

                // Fetch roles and pharmacy details asynchronously
                var pharmacyTask = _pharmacyRepository.GetAsync(p => p.AccountId == user.Id);
                // Get roles asynchronously
                var rolesTask = _userManager.GetRolesAsync(user);

                // Wait for both tasks to complete
                await Task.WhenAll(rolesTask, pharmacyTask).ConfigureAwait(false);

                var roles = await rolesTask;
                var pharmacy = await pharmacyTask;

                List<Claim> claims = BuildClaims(user, roles, pharmacy);

                JwtSecurityToken token = GenerateJwtToken(claims, loginInfo.RememberMe);

                return Results.Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    userName = user.UserName,
                });
            }
            catch (Exception ex)
            {
                // Log the exception here (add logging service)
                return Results.Problem("An error occurred during login.");
            }
        }

        private async Task CreateUserProfileAsync(Account account, RegisterAccountDTO accountDto)
        {
            if (accountDto.Patient != null)
            {
                account.Patient = _mapper.Map<Patient>(accountDto.Patient);
                account.Patient.AccountId = account.Id;
                await _patientRepository.CreateAsync(account.Patient).ConfigureAwait(false);
            }
            else if (accountDto.Pharmacy != null)
            {
                account.Pharmacy = _mapper.Map<Pharmacy>(accountDto.Pharmacy);
                account.Pharmacy.AccountId = account.Id;
                await _pharmacyRepository.CreateAsync(account.Pharmacy).ConfigureAwait(false);
            }
        }
        private List<Claim> BuildClaims(Account user, IList<string> roles, Pharmacy pharmacy)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            // Initialize claims list
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),   // JWT ID
            };
            // Add pharmacy-specific claims if user is a pharmacy
            if (pharmacy != null)
            {
                claims.Add(new Claim(CustomClaimTypes.PharmacyId, pharmacy.PharmacyID.ToString()));
            }

            // Add roles
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            return claims;
        }
        private JwtSecurityToken GenerateJwtToken(List<Claim> claims, bool rememberMe = false)
        {
            SigningCredentials signInCred = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]!)),
            SecurityAlgorithms.HmacSha256);
            // Set token expiration based on rememberMe flag
            DateTime expiration;
            if (rememberMe)
            {
                expiration = DateTime.UtcNow.AddDays(int.Parse(_config["JWT:RefreshTokenExpirationDays"]!));
            }
            else
            {
                expiration = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT:ExpirationMinutes"]!));
            }
            return new JwtSecurityToken(
                    issuer: _config["JWT:Issuer"],
                    audience: _config["JWT:Audience"],
                    claims: claims,
                    expires: expiration,
                    signingCredentials: signInCred);
        }
    }
}
