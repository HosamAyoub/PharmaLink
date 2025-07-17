using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;
using PharmaLink_API.Repository.IRepository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PharmaLink_API.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<Account> _userManager;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        //private readonly AccountDTO newUser;
        public AccountRepository(UserManager<Account> userManager, IMapper mapper, ApplicationDbContext db, IConfiguration configuration)
        {
            _userManager = userManager;
            _mapper = mapper;
            _db = db;
            _config = configuration;
            //newUser = new AccountDTO();
        }
        public async Task<IdentityResult> RegisterAsync(RegisterAccountDTO account)
        {
            if (account == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User cannot be null" });
            }
            var transaction = await _db.Database.BeginTransactionAsync();
            Account newAccount = _mapper.Map<Account>(account);
            IdentityResult result = await _userManager.CreateAsync(newAccount, account.PasswordHash);

            if (!result.Succeeded)
            {
                return result;
            }

            if (account.Patient != null)
            {
                account.Patient.AccountId = newAccount.Id;
                newAccount.Patient = _mapper.Map<Patient>(account.Patient);
                await _db.Patients.AddAsync(newAccount.Patient);
            }
            else if (account.Pharmacy != null)
            {
                account.Pharmacy.AccountId = newAccount.Id;
                newAccount.Pharmacy = _mapper.Map<Pharmacy>(account.Pharmacy);
                await _db.Pharmacies.AddAsync(newAccount.Pharmacy);
            }
            else if (account.Patient != null)
            {
                account.Patient.AccountId = newAccount.Id;
                newAccount.Patient = _mapper.Map<Patient>(account.Patient);
                await _db.Patients.AddAsync(newAccount.Patient);
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

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

        //public Task<Account> GetUserByEmailAsync(string email)
        //{
        //    if (string.IsNullOrEmpty(email))
        //    {
        //        return Task.FromResult<Account>(null);
        //    }
        //    return _userManager.FindByEmailAsync(email);
        //}
        //public Task<Account> GetUserByEmailAsync(string email)
        //{
        //    if (string.IsNullOrEmpty(email))
        //    {
        //        return Task.FromResult<Account>(null);
        //    }
        //    return _userManager.FindByEmailAsync(email);
        //}
        //public Task<Account> GetUserByUsernameAsync(string username)
        //{
        //    if (string.IsNullOrEmpty(username))
        //    {
        //        return Task.FromResult<Account>(null);
        //    }
        //    return _userManager.FindByNameAsync(username);
        //}

        //public List<Account> GetUserByDisplayNameAsync(string displayName)
        //{
        //    if (string.IsNullOrEmpty(displayName))
        //    {
        //        return null;
        //    }
        //    return _db.Users
        //        .Where(u => u. != null && u.DisplayName.Contains(displayName, StringComparison.OrdinalIgnoreCase))
        //        .ToList();
        //}

        //public Task<IdentityResult> UpdateUserAsync(Account user)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<IdentityResult> DeleteUserAsync(string id)
        //{
        //    throw new NotImplementedException();
        //}

    }
}
