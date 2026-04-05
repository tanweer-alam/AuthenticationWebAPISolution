using API.DTOs;
using API.Models;

namespace API.Data
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(RegistrationDto registerDto);
        Task<AuthResponseDto?> AuthenticateUserAsync(LoginDto loginDto, User user, string ipAddress);
        
    }
}
