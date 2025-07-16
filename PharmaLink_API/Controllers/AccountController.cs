using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.AccountDTO;
using PharmaLink_API.Repository.IRepository;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public async Task<IActionResult> Register(AccountDTO userRegisterInfo)
        {
            IdentityResult result = IdentityResult.Success;
            if (ModelState.IsValid)
            {
                result = await _accountRepository.RegisterAsync(userRegisterInfo);
                if (result.Succeeded)
                {
                    return Ok(new { Message = $"User registered successfully\n{userRegisterInfo.UserName}\n{userRegisterInfo.Email}" });
                }
                //else
                //{
                //    foreach (var error in result.Errors)
                //    {
                //        ModelState.AddModelError(string.Empty, error.Description);
                //    }
                //}
            }
            return BadRequest(result.Errors.ToArray());
        }
        //[HttpPost("Login")]

    }
}
