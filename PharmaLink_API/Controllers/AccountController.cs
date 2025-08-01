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

    }
}
