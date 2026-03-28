using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace API.Middleware
{
    public class SecondMiddleware
    {
        private readonly IMemoryCache _memoryCache; //since it is singleton, it can be injected in the constructor, but scoped and transient services must be resolved in the InvokeAsync method
        private readonly RequestDelegate _next;

        public SecondMiddleware(IMemoryCache memoryCache, RequestDelegate next)
        {
            _memoryCache = memoryCache;
            _next = next;
        }

        //Scoped and transient services must be resolved in the InvokeAsync method
        public async Task Invoke(HttpContext context, IMessageWriter writer)
        {
            var message = $"[SecondMiddleware] {context.Request.Method} {context.Request.Path}";
            Debug.WriteLine(message);
            Console.WriteLine(message);

            var cacheKey = _memoryCache.Get<string>("TestCache") ?? "No value in cache";
            _memoryCache.Set("SecondMiddlewareCache", $"Value from SecondMiddleware: {cacheKey}", new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            writer.Write(DateTime.Now.Ticks.ToString());
            await _next(context);
        }
    }
    public static class SecondMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecondMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SecondMiddleware>();
        }
    }
    public interface IMessageWriter
    {
        void Write(string message);
    }
    //this is a scoped service, so it should be injected in the middleware method Async, not in the constructor, because middleware is a singleton
    public class LoggingMessageWriter : IMessageWriter
    {

        private readonly ILogger<LoggingMessageWriter> _logger;

        public LoggingMessageWriter(ILogger<LoggingMessageWriter> logger) =>
            _logger = logger;

        public void Write(string message) =>
            _logger.LogInformation(message);
    }
}
