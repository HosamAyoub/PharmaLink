using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.AccountDTO;

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly UserManager<Account> _userManager;
        private readonly IMapper _mapper;

        public AccountController(UserManager<Account> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterUserDTO userRegisterInfo)
        {
            if (ModelState.IsValid)
            {
                //Account newUser = _mapper.Map<Account>(userRegisterInfo);
                Account newUser = new Account
                {
                    UserName = userRegisterInfo.UserName,
                    Email = userRegisterInfo.Email,
                    // You can set other properties here if needed
                };
                IdentityResult result = await _userManager.CreateAsync(newUser, userRegisterInfo.Password);
                if (result.Succeeded)
                {

                    // Optionally, you can add the user to a role here
                    // await _userManager.AddToRoleAsync(newUser, "User");
                    return Ok(new { Message = $"User registered successfully\n{newUser.UserName}\n{newUser.Email}" });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                return BadRequest(result.Errors.ToList());
            }
            return BadRequest(ModelState);
        }
        //[HttpPost("Login")]

    }
}
