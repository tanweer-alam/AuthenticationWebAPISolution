using Microsoft.Extensions.Caching.Memory;

namespace API.Middleware
{
    //The middleware is registered as a scoped or transient service in the app's service container.
    //IMiddleware is activated per client request (connection),
    //so scoped services can be injected into the middleware's constructor.
    public class HMACAuthenticationMiddleware : IMiddleware
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly IMessageWriter _writer;

        public HMACAuthenticationMiddleware(IMemoryCache memoryCache, IConfiguration configuration, IMessageWriter writer)
        {
            _memoryCache = memoryCache;
            _configuration = configuration;
            _writer = writer;
        }
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var testCaseValue = _configuration["Authentication:TestCaseValue"] ?? "DefaultTestValue";
            var cacheKey = "TestCache";
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _memoryCache.Set(cacheKey, testCaseValue, cacheOptions);
            _writer.Write(DateTime.Now.Ticks.ToString());

            next(context);

            Console.WriteLine("We are retreating");
            return Task.CompletedTask;
        }
    }
    public static class HMACAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseHMACAuthentication(this IApplicationBuilder app)
        {
            return app.UseMiddleware<HMACAuthenticationMiddleware>();
        }
    }
}
