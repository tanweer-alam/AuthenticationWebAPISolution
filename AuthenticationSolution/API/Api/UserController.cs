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
        public UserController(UserDbContext dbContext)
        {
            _dbContext = dbContext;
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
            //compute password
            PasswordHasher.CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] saltHash);
            var user = new User
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = saltHash
            };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
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
            if(!PasswordHasher.VerifyPasswordHash(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized("Incorrect password");
            }
            return Ok(new {Message = "Login successfull"});
        }
    }
}
