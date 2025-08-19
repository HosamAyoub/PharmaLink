using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PharmaLink_API.Core.Constants;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.LoginAccoutDTO;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Web;


namespace PharmaLink_API.Services
{
    // Service for handling account registration, login, and related logic
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly UserManager<Account> _userManager;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IRoleService _roleService;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Interfaces.IEmailSender emailSender;

        // Inject dependencies for account, role, and mapping operations
        public AccountService(UserManager<Account> userManager, IMapper mapper,
            IConfiguration configuration, 
            IAccountRepository accountRepository,
            IPatientRepository patientRepository, 
            IPharmacyRepository pharmacyRepository,
            IRoleService roleService, IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            Interfaces.IEmailSender emailSender)
        {
            _userManager = userManager;
            _mapper = mapper;
            _config = configuration;
            _accountRepository = accountRepository;
            _patientRepository = patientRepository;
            _pharmacyRepository = pharmacyRepository;
            _roleService = roleService;
            _WebHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            this.emailSender = emailSender;
        }

        /// <summary>
        /// Registers a new user account, creates a profile, assigns a role, and manages the transaction.
        /// </summary>
        /// <param name="accountDto">The registration data for the new user.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        public async Task<IdentityResult> RegisterAsync(RegisterAccountDTO accountDto)
        {
            // Validate input
            if (accountDto == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User cannot be null" });
            }

            // Start a new transaction for the registration process
            var transaction = await _accountRepository.StartTransactionAsync().ConfigureAwait(false);
            try
            {
                // Check if the email is already registered
                var existingUser = await _userManager.FindByEmailAsync(accountDto.Email).ConfigureAwait(false);
                if (existingUser != null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Email is already registered" });
                }

                // Map the DTO to an Account entity and create the user
                Account newAccount = _mapper.Map<Account>(accountDto);
                IdentityResult result = await _userManager.CreateAsync(newAccount, accountDto.PasswordHash).ConfigureAwait(false);
                // If user creation fails, rollback and return the result
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    return result;
                }

                // Create the user profile (Patient or Pharmacy or Admin) and assign the role in parallel
                var profileTask = CreateUserProfileAsync(newAccount, accountDto);
                var roleTask = _roleService.assignRoleAsync(accountDto, newAccount);

                // Wait for both tasks to complete
                await Task.WhenAll(profileTask, roleTask).ConfigureAwait(false);

                // Check if role assignment succeeded
                var roleResult = roleTask.Result;
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    return roleResult;
                }
                // Generate email confirmation token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(newAccount);
                var encodedToken = HttpUtility.UrlEncode(token);

                // Create confirmation link
                var confirmationLink = $"http://localhost:4200/confirm-email?userId={newAccount.Id}&token={encodedToken}";

                // Send email
                await emailSender.sendEmailAsync(accountDto.Email, "Confirm Your Email",
                    $"Please confirm your email by clicking <a href='{confirmationLink}'>here</a>.");


                // Save all changes and commit the transaction
                await _accountRepository.SaveAsync().ConfigureAwait(false);
                await _accountRepository.EndTransactionAsync().ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                // Rollback the transaction if any error occurs
                await transaction.RollbackAsync().ConfigureAwait(false);
                return IdentityResult.Failed(new IdentityError { Description = $"An error occurred while registering the account: {ex.Message}" });
            }
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token if successful.
        /// </summary>
        /// <param name="loginInfo">The login credentials.</param>
        /// <returns>IResult with token or error message.</returns>
        public async Task<IResult> LoginAsync(LoginDTO loginInfo)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(loginInfo.Email) || string.IsNullOrWhiteSpace(loginInfo.Password))
            {
                return Results.BadRequest("Email or password cannot be empty.");
            }
            try
            {
                // Find the user by email
                var user = await _userManager.FindByEmailAsync(loginInfo.Email).ConfigureAwait(false);
                if (user == null)
                {
                    return Results.Unauthorized();
                }
                if (!await _userManager.IsEmailConfirmedAsync(user))
                    return Results.Unauthorized();

                // Check the password
                var authenticated = await _userManager.CheckPasswordAsync(user, loginInfo.Password).ConfigureAwait(false);
                if (!authenticated)
                {
                    return Results.Unauthorized();
                }

                // Get user roles and pharmacy information
                var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
                var pharmacy = await _pharmacyRepository.GetAsync(
                    filter: p => p.AccountId == user.Id,
                    tracking: false
                ).ConfigureAwait(false);

                // Build claims for the JWT token
                List<Claim> claims = BuildClaims(user, roles, pharmacy);

                // Generate the JWT token
                JwtSecurityToken token = GenerateJwtToken(claims, loginInfo.RememberMe);

                // Return the token and user info
                return Results.Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    userName = user.UserName,
                    role = roles
                });
            }
            catch (Exception ex)
            {
                // Log the exception here (add logging service)
                return Results.Problem("An error occurred during login.");
            }
        }

        /// <summary>
        /// Creates a user profile (Patient or Pharmacy) based on the registration data.
        /// </summary>
        /// <param name="account">The account entity.</param>
        /// <param name="accountDto">The registration data.</param>
        private async Task CreateUserProfileAsync(Account account, RegisterAccountDTO accountDto)
        {
            // If the user is a patient, create a patient profile
            if (accountDto.Patient != null)
            {
                account.Patient = _mapper.Map<Patient>(accountDto.Patient);
                account.Patient.AccountId = account.Id;
                await _patientRepository.CreateAsync(account.Patient).ConfigureAwait(false);
            }
            // If the user is a pharmacy, create a pharmacy profile
            else if (accountDto.Pharmacy != null)
            {
                account.Pharmacy = _mapper.Map<Pharmacy>(accountDto.Pharmacy);
                account.Pharmacy.AccountId = account.Id;
                if (accountDto.Pharmacy.Doc != null && accountDto.Pharmacy.Doc.Length > 0)
                {
                    var docUrl = await SaveDocumentAsync(accountDto.Pharmacy.Doc);
                    account.Pharmacy.DocURL = docUrl;
                }
                await _pharmacyRepository.CreateAsync(account.Pharmacy).ConfigureAwait(false);
            }
        }

        // Update the constructor to inject IHttpContextAccessor

        private async Task<string> SaveDocumentAsync(IFormFile docFile)
        {
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(docFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Only PDF documents are allowed");

            var fileName = Guid.NewGuid() + extension;
            var filePath = Path.Combine(_WebHostEnvironment.WebRootPath, "documents", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using var stream = new FileStream(filePath, FileMode.Create);
            await docFile.CopyToAsync(stream);

            // Use IHttpContextAccessor to access the current HTTP context
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                throw new InvalidOperationException("Unable to access the current HTTP request.");

            return $"{request.Scheme}://{request.Host}/documents/{fileName}";
        }

        /// <summary>
        /// Builds a list of claims for the JWT token based on user and role information.
        /// </summary>
        /// <param name="user">The account entity.</param>
        /// <param name="roles">The user's roles.</param>
        /// <param name="pharmacy">Pharmacy info if the user is a pharmacy.</param>
        /// <returns>List of claims for the JWT token.</returns>
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
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
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

        /// <summary>
        /// Generates a JWT token for the authenticated user.
        /// </summary>
        /// <param name="claims">The claims to include in the token.</param>
        /// <param name="rememberMe">Whether to use a longer expiration for the token.</param>
        /// <returns>JwtSecurityToken object.</returns>
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

        /// <summary>
        /// Verifies a JWT token and returns user information if valid.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IResult> VerifyTokenAsync([FromBody] string token)
        {
            // Validate the token
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.BadRequest("Token cannot be empty.");
            }
            // Create a token handler to validate the JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var key = Encoding.UTF8.GetBytes(_config["JWT:Key"]!);
                var validationParameters = new TokenValidationParameters
                {
                    // Ensures the token’s signature is checked
                    ValidateIssuerSigningKey = true,
                    // The signing key used to validate the token
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    // Validates the issuer of the token
                    ValidateIssuer = true,
                    // The expected issuer of the token
                    ValidIssuer = _config["JWT:Issuer"],

                    // Validates the audience of the token
                    ValidateAudience = true,
                    // The expected audience of the token
                    ValidAudience = _config["JWT:Audience"],

                    // Validates the token's expiration
                    ValidateLifetime = true,
                    // Clock skew to account for time differences
                    ClockSkew = TimeSpan.Zero
                };

                // Validate the token and extract the principal
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Extract User Information from Claims
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                // fetch user information from the database
                var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);

                if (user == null)
                {
                    return Results.Unauthorized();
                }
                // Return the user information if the token is valid
                return Results.Ok(new
                {
                    token,
                    expiration = validatedToken.ValidTo,
                    userName = user.UserName,
                    role = roles
                });
            }
            catch (SecurityTokenException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                // Log the exception here (add logging service)
                return Results.Problem("An error occurred while verifying the token.");
            }

        }

        public async Task<bool?> EmailIsConfirmedAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            try
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                return result.Succeeded;
            }
            catch (Exception)
            {
                // Better: log exception instead of rethrowing
                return false;
            }
        }

    }
}