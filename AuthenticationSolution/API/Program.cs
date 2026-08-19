using API.Data;
using API.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddMemoryCache();

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, API.AuthSchemes.BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddSingleton<IAuthorizationHandler, API.Authorization.HierarchicalRolesAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.Requirements.Add(new RolesAuthorizationRequirement(new[] { "Admin", "User" }));
    });
});
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//even commented it will be called because of the convention based middleware, but it is a good practice to call it explicitly in the pipeline.
app.UseAuthentication();

app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseHMACAuthentication();
});
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
