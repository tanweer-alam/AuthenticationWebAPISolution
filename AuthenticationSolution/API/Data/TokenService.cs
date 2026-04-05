using API.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Data
{
    public class TokenService : ITokenService
    {
        private readonly ClientSecretService _clientSecretService;
        private readonly IConfiguration _configuration;
        public TokenService(ClientSecretService clientSecretService, IConfiguration configuration)
        {
            _clientSecretService = clientSecretService;
            _configuration = configuration;
        }
        public string GenerateAccessToken(User user, out string jwtId, string clientCode, string audience)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            //client secret (stored as Base64 in DB). Decode to raw bytes for signing key.
            var key = _clientSecretService.GetClientSecretKeyAsync(clientCode).Result;
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException($"Client secret for '{clientCode}' not found.");
            var keyBytes = Convert.FromBase64String(key);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            jwtId = Guid.NewGuid().ToString();

            var issuer = _configuration["JwtSettings:Issuer"] ?? "DefaultIssuer";
            var accessTokenExpirationMinutes = int.TryParse(_configuration["JwtSettings:AccessTokenExpirationMinutes"], out var val) ? val : 15;

            //set claims
            // Define the claims to be embedded in the JWT token
            var claims = new List<Claim>
            {
                // Subject claim represents user identifier
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                // JWT ID claim for unique token identification (used to link refresh tokens)
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
                // User email claim for identification purposes
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                // Issuer claim indicating the token issuer
                new Claim(JwtRegisteredClaimNames.Iss, issuer),
                // Audience claim specifying the client URL expected to receive the token
                new Claim(JwtRegisteredClaimNames.Aud, audience),
                // Custom claim specifying the client id (helps identify which client requested the token)
                new Claim("client_id", clientCode),
                new Claim(ClaimTypes.Role, user.Role) // Include user role as a claim
            };

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
                SigningCredentials = credentials,
                Issuer = issuer,
                Audience = audience
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
    }
}
