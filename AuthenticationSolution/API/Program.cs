using API.Data;
using API.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#region Service Registration / DI container

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//this creates scoped instance of UserDbContext for each request, and it will be disposed of at the end of the request
builder.Services.AddDbContext<UserDbContext>(optionsBuilder =>
    optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, API.AuthSchemes.BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddSingleton<IAuthorizationHandler, API.Authorization.HierarchicalRolesAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.Requirements.Add(new RolesAuthorizationRequirement(new[] { "Admin" , "User" }));
    });
});

builder.Services.AddScoped<HMACAuthenticationMiddleware>(); //needed as this can be trans or scoped
//builder.Services.AddSingleton<API.Middleware.SecondMiddleware>(); not needed, by default singleton
builder.Services.AddScoped<IMessageWriter, LoggingMessageWriter>();
builder.Services.AddMemoryCache();

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseWhen(context => context.Request.Query.ContainsKey("branch"), handleBranch =>
{
    handleBranch.Use(async (context, next) =>
    {
        var branchValue = context.Request.Query["branch"].ToString();
        Console.WriteLine($"[BranchMiddleware] Branch value: {branchValue}");
        await context.Response.WriteAsync($"[BranchMiddleware] Branch value: {branchValue}\n");
        await next.Invoke(context);
    });
});
app.Map("/map1", HandleMap1);
app.Map("/map2", handleMap2 =>
{
    handleMap2.Run(async context =>
    {
        await context.Response.WriteAsync("map2 handled");
    });
});

//app.UseMiddleware<HMACAuthenticationMiddleware>();
app.UseHMACAuthentication();
//app.UseMiddleware<SecondMiddleware>();
app.UseSecondMiddleware();
app.Use(async (context, next) =>
{
    Console.WriteLine($"[InlineMiddleware] {context.Request.Method} {context.Request.Path}");
    await next.Invoke(context);
});

//app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{id?}");

app.MapControllers();

app.Run();

static void HandleMap1(IApplicationBuilder app)
{
    app.Use(async (context, next) =>
    {
        Console.WriteLine("Map1 - Middleware 1 before next");
        await context.Response.WriteAsync("Map1 - Middleware 1 before next\n");
        await next.Invoke();
        Console.WriteLine("Map1 - Middleware 1 after next");
    });
    app.Run(async context =>
    {
        Console.WriteLine("Map1 handled");
        await context.Response.WriteAsync("Map1 handled");
    });
}