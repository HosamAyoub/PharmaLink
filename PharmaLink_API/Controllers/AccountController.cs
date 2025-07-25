using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;
using PharmaLink_API.Models.DTO.LoginAccoutDTO;
using PharmaLink_API.Repository.Interfaces;

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;

        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterAccountDTO userRegisterInfo)
        {
            IdentityResult result = IdentityResult.Success;
            if (ModelState.IsValid)
            {
                result = await _accountRepository.RegisterAsync(userRegisterInfo);
                if (result.Succeeded)
                {
                    return Ok(new { Message = $"User registered successfully\n{userRegisterInfo.UserName}\n{userRegisterInfo.Email}" });
                }
            }
            return BadRequest(result.Errors.ToArray());
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO loginInfo)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountRepository.LoginAsync(loginInfo.Email, loginInfo.Password);
                if (result is not null)
                {
                    return Ok(result);
                }
            }
            return Unauthorized(new { Message = "Invalid email or password." });
        }

    }
}
