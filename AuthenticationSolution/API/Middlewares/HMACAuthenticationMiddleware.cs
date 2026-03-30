using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace API.Middlewares
{
    //security mechanism used to verify the integrity and authenticity of a message exchanged between a client and a server.
    //Server PART
    //cryptographic hash function and a secret key to generate a unique hash value (HMAC) for each message.
    //Convention based middleware
    //Singleton Middleware
    public class HMACAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly TimeSpan NonceExpiry = TimeSpan.FromMinutes(5);

        //singleton services will be resolved in the constructor
        public HMACAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, IMemoryCache memoryCache)
        {
            _next = next;
            _configuration = configuration;
            _memoryCache = memoryCache;
        }
        //scoped services will be resolved in this method
        public async Task InvokeAsync(HttpContext httpContext, ClientSecretService clientSecretService)
        {
            //server will verify the HMAC signature included in the request header
            //against the expected signature generated using the shared secret key and the request payload.

            /*
            Checks if HMAC is enabled.
            Validates the presence and structure of the Authorization header.
            Extracts HMAC token, nonce, timestamp, and clientId from headers.
            Verifies client, timestamp freshness and nonce uniqueness to prevent replay attacks.
            Recomputes HMAC signature using the same hashing logic as the client.
            Allows or rejects the request accordingly.
             */

            //hmac enalbed check
            bool isHMACEnalbed = _configuration.GetValue<bool>("HMACSettings:IsHMACEnabled");
            if (isHMACEnalbed == false)
            {
                await _next(httpContext);
                return;
            }
            //header validation
            if (httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader) == false)
            {
                httpContext.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync("Authorization header is missing");
                return;
            }
            if (authHeader.ToString().StartsWith("HMAC", StringComparison.OrdinalIgnoreCase) == false)
            {
                httpContext.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync("Invalid Authorization header format");
                return;
            }
            //extract clientId, token, nonce and timestamp
            var tokenParts = authHeader.ToString().Substring(4).Split('|');
            if(tokenParts.Length != 4)
            {
                httpContext.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync("Invalid HMAC token format");
                return;
            }
            var clientCode = tokenParts[0];
            var token = tokenParts[1];
            var nonce = tokenParts[2];
            var timestamp = tokenParts[3];
            //valid clientid
            var clientSecretKey = await clientSecretService.GetClientSecretKeyAsync(clientCode);
            if(string.IsNullOrEmpty(clientSecretKey))
            {
                httpContext.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync("Invalid client ID");
                return;
            }
            //validate timestamp
            if(long.TryParse(timestamp, out var timestampSeconds) == false)
            {
                httpContext.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync("Invalid timestamp format");
                return;
            }
            // Convert the timestamp to Unix Time or EPOC Time
            var requestTime = DateTimeOffset.FromUnixTimeSeconds(timestampSeconds).UtcDateTime;
            var currentTime = DateTime.UtcNow;
            // Check if the timestamp is within the allowed timeframe (within 5 minutes)
            // This is to avoid Reply Attack
            if (Math.Abs((currentTime - requestTime).TotalMinutes) > 5)
            {
                httpContext.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync("Timestamp is outside the allowable range");
                return;
            }

            //validate nonce
            var nonceKey = $"HMACNonce:{clientCode}:{nonce}";
            if(_memoryCache.TryGetValue(nonceKey, out _))
            {
                httpContext.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync("Nonce has already been used");
                return;
            }
            _memoryCache.Set(nonceKey, true, NonceExpiry);

            var requestBody = string.Empty;
            if (httpContext.Request.Method == HttpMethods.Post || httpContext.Request.Method == HttpMethods.Put)
            {
                httpContext.Request.EnableBuffering();
                using(var reader = new StreamReader(httpContext.Request.Body, System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    httpContext.Request.Body.Position = 0;
                }
            }
            //validate HMAC token
            bool isValidHMAC = ValidateToken(token, nonce, timestamp, httpContext.Request, requestBody, clientSecretKey);
            if(isValidHMAC == false)
            {
                httpContext.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync("Invalid HMAC token");
                return;
            }
            await _next(httpContext);
        }

        private bool ValidateToken(string token, string nonce, string timestamp, HttpRequest request, string requestBody, string secretKey)
        {
            var path = request.Path.ToString();
            var requestContent = new StringBuilder()
                .Append(request.Method.ToUpper())
                .Append(path.ToUpper())
                .Append(nonce)
                .Append(timestamp);
            if(request.Method == HttpMethods.Post || request.Method == HttpMethods.Put)
            {
                requestContent.Append(requestBody);
            }
            var secretKeyEncoded = Encoding.UTF8.GetBytes(secretKey);
            var requestContentEncoded = Encoding.UTF8.GetBytes(requestContent.ToString());
            using(var hmac = new HMACSHA512(secretKeyEncoded))
            {
                var computedHash = hmac.ComputeHash(requestContentEncoded);
                var computedToken = Convert.ToBase64String(computedHash);
                return computedToken == token;
            }
        }
    }

    //Extension method to add the middleware to the HTTP request pipeline.
    public static class HMACAuthenticationMiddlewareExtension
    {
        public static IApplicationBuilder UseHMACAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HMACAuthenticationMiddleware>();
        }
    }

}
