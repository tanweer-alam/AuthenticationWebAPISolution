using API.DTOs;
using API.Helpers;
using API.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly UserDbContext _dbContext;
        private readonly ITokenService _tokenService;
        public UserService(IConfiguration configuration, UserDbContext dbContext, ITokenService tokenService)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _tokenService = tokenService;
        }
        public Task<AuthResponseDto?> AuthenticateUserAsync(LoginDto loginDto, User user, string ipAddress)
        {
            if(_configuration.GetValue<bool>("BasicAuthSettings:IsBasicAuthEnabled") == true)
            {
                if (!PasswordHasher.VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return Task.FromResult<AuthResponseDto?>(null);
                }
            }
            else if(_configuration.GetValue<bool>("JwtSettings:IsJwtEnabled") == true)
            {
                var accessToken = _tokenService.GenerateAccessToken(user, out string jwtId, loginDto.ClientCode, ipAddress);
                var accessTokenExpiryMinutes = int.TryParse(_configuration["JwtSettings:AccessTokenExpirationMinutes"], out var val) ? val : 15;
                // Return the tokens and expiry info encapsulated in AuthResponseDTO
                var result = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes)
                };
                return Task.FromResult<AuthResponseDto?>(result);
            }
            return Task.FromResult<AuthResponseDto?>(null);
        }

        public async Task<User> RegisterUserAsync(RegistrationDto model)
        {
            PasswordHasher.CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] saltHash);
            var user = new User
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = saltHash,
                Role = "User"
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }
    }
}
