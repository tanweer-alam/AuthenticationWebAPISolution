using API.Models;

namespace API.Data
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, out string jwtId, string clientCode, string audience);
    }
}
