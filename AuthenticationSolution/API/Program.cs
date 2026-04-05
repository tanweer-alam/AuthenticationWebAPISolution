using API.Data;
using API.Middlewares;
using API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#region Service Registration / DI container
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<UserDbContext>(optionsBuilder =>
optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddScoped<ClientSecretService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IClientCacheService, ClientCacheService>();
builder.Services.AddMemoryCache();

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, API.AuthSchemes.BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddSingleton<IAuthorizationHandler, API.Authorization.HierarchicalRolesAuthorizationHandler>();

Lazy<IClientCacheService>? clientCacheInstance = null;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "DefaultIssuer",
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            // fetching the corresponding client’s secret key from cache.
            IssuerSigningKeyResolver = (token, securityToken, kid, validationParameter) =>
            {
                var jwtToken = new JwtSecurityToken(token);
                var clientCode = jwtToken.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;
                if (string.IsNullOrEmpty(clientCode) || clientCacheInstance == null)
                    return Enumerable.Empty<SecurityKey>();
                // Retrieve the client info synchronously from cache
                var client = clientCacheInstance.Value.GetClientByClientIdAsync(clientCode).Result;
                if (client == null)
                    return Enumerable.Empty<SecurityKey>();
                // Convert the client's stored Base64 secret into a byte array for key
                var keyBytes = Convert.FromBase64String(client.SecretKey);
                // Create the symmetric security key from byte array for signature validation
                return new[] { new SymmetricSecurityKey(keyBytes) };
            }
        };
        // Additional asynchronous validation after the token is validated,
        // confirming the client exists and audience matches the stored client URL.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                // Extract client_id claim from the validated token
                var clientId = context.Principal?.FindFirst("client_id")?.Value;
                if (string.IsNullOrEmpty(clientId))
                {
                    // Fail if claim is missing
                    context.Fail("ClientId claim missing.");
                    return;
                }
                if (clientCacheInstance == null)
                {
                    context.Fail("Client Cache Instance is null");
                    return;
                }
                // Asynchronously get client info from cache or database
                var client = await clientCacheInstance.Value.GetClientByClientIdAsync(clientId);
                if (client == null)
                {
                    // Fail if client not found
                    context.Fail("Invalid client.");
                    return;
                }
                // Extract audience claim from token and compare to client URL stored in DB/cache
                var audClaim = context.Principal?.FindFirst(JwtRegisteredClaimNames.Aud)?.Value;
                
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.Requirements.Add(new RolesAuthorizationRequirement(new[] { "Admin", "User" }));
    });
});
#endregion

var app = builder.Build();


// Initialize the lazy client cache instance now that the DI container is built and available
clientCacheInstance = new Lazy<IClientCacheService>(() =>
    app.Services.GetRequiredService<IClientCacheService>());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//even commented it will be called because of the convention based middleware, but it is a good practice to call it explicitly in the pipeline.
app.UseAuthentication();

//app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder =>
//{
//    appBuilder.UseHMACAuthentication();
//});
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
