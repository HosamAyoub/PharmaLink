using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;
using PharmaLink_API.Models.DTO.LoginAccoutDTO;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        // Inject the account service to handle business logic
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="userRegisterInfo">The registration data for the new user.</param>
        /// <returns>Action result indicating success or failure.</returns>
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterAccountDTO userRegisterInfo)
        {
            IdentityResult result = IdentityResult.Success;
            // Validate the incoming model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Call the service to register the user
            result = await _accountService.RegisterAsync(userRegisterInfo);

            // Check if registration succeeded
            if (result.Succeeded)
            {
                return Ok(new { Message = "Registration successful. Please check your email to confirm." });
            }
            // Return errors if registration failed
            return BadRequest(result.Errors.ToArray());
        }

        [HttpPost("RegisterPharmacy")]
        public async Task<IActionResult> RegisterPharmacy([FromForm] RegisterAccountDTO userRegisterInfo)
        {
            IdentityResult result = IdentityResult.Success;
            // Validate the incoming model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Call the service to register the user
            result = await _accountService.RegisterAsync(userRegisterInfo);

            // Check if registration succeeded
            if (result.Succeeded)
            {
                return Ok(new { Message = $"User registered successfully\n{userRegisterInfo.UserName}\n{userRegisterInfo.Email}" });
            }
            // Return errors if registration failed
            return BadRequest(result.Errors.ToArray());
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token if successful.
        /// </summary>
        /// <param name="loginInfo">The login credentials.</param>
        /// <returns>Action result with token or error message.</returns>
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO loginInfo)
        {
            // Validate the incoming model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Call the service to authenticate the user
            var result = await _accountService.LoginAsync(loginInfo);

            // If authentication is successful, return the result
            if (result is not null)
            {
                return Ok(result);
            }
            // Otherwise, return unauthorized
            return Unauthorized(new { Message = "Invalid email or password." });
        }

        /// <summary>
        /// Verifies a JWT token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("VerifyToken")]
        public async Task<IActionResult> VerifyToken([FromBody] string token)
        {
            // Validate the incoming token
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { Message = "Token is required." });
            }
            // Call the service to verify the token
            var result = await _accountService.VerifyTokenAsync(token);
            // If verification is successful, return the result
            if (result is not null)
            {
                return Ok(result);
            }
            // Otherwise, return unauthorized
            return Unauthorized(new { Message = "Invalid or expired token." });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            // 1. Validate input
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Invalid email confirmation request.");

            bool? isConfirmed = await _accountService.EmailIsConfirmedAsync(userId, token);
            if( isConfirmed == null)
                return BadRequest(new { messsage ="Invalid confirmation token or user ID." });
            if( isConfirmed == false)
                return BadRequest(new{ message = "Email confirmation failed. Token may be invalid or expired." });
            if( isConfirmed == true)
                return Ok(new { message = "Email already confirmed." });

            return BadRequest("Email confirmation failed.");
        }
    }
}