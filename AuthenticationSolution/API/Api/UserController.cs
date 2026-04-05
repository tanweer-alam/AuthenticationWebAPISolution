using API.Data;
using API.DTOs;
using API.Helpers;
using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _dbContext;
        private readonly IUserService _userService;
        public UserController(UserDbContext dbContext, IUserService userService)
        {
            _dbContext = dbContext;
            _userService = userService;
        }

        //registration: POST api/user/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegistrationDto model)
        {
            //validate model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check duplicate
            if(await _dbContext.Users.AnyAsync(x => x.Email == model.Email))
            {
                return BadRequest("Email already in use");
            }
            var user = await _userService.RegisterUserAsync(model);

            return Ok(new {user.FirstName, user.LastName, user.Email});
        }

        //login: POST api/user/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == model.Email);
            if(user == null)
            {
                return Unauthorized("Invalid email");
            }
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var result = await _userService.AuthenticateUserAsync(model, user, ipAddress);
            if (result == null)
            {
                return Unauthorized("Incorrect password");
            }
            return Ok(result);
        }
    }
}
