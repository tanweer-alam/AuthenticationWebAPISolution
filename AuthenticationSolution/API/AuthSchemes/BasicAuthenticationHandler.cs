using API.Data;
using API.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace API.AuthSchemes
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly UserDbContext _dbContext;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger, 
            UrlEncoder encoder,
            UserDbContext dbContext) : base(options, logger, encoder)
        {
            _dbContext = dbContext;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                if (!Request.Headers.ContainsKey("Authorization"))
                {
                    return AuthenticateResult.Fail("Missing Authorization Header");
                }
                var authorizationHeader = Request.Headers["Authorization"].ToString();
                if (!AuthenticationHeaderValue.TryParse(authorizationHeader, out var headerValue))
                { 
                    return AuthenticateResult.Fail("Invalid Authorization Header");
                }
                if(headerValue.Scheme != "Basic")
                {
                    return AuthenticateResult.Fail("Invalid Authorization Scheme");
                }
                var credentialsBytes = Convert.FromBase64String(headerValue.Parameter);
                var credentials = System.Text.Encoding.UTF8.GetString(credentialsBytes).Split(':', 2);
                if (credentials.Length != 2)
                {
                    return AuthenticateResult.Fail("Invalid Authorization Header");
                }

                var email = credentials[0];
                var password = credentials[1];
                
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
                if(user == null || !PasswordHasher.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                {
                    return AuthenticateResult.Fail("Invalid Email or Password");
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };
                
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception)
            {
                return AuthenticateResult.Fail("An error occurred while processing the authentication request");
            }
        }
    }
}
