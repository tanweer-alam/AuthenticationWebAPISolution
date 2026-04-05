using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace API.Data
{
    public class ClientCacheService : IClientCacheService
    {
        // Prefix to uniquely identify each client entry in the cache
        private const string CacheKeyPrefix = "Client_";
        // Service provider to create scopes for scoped services like DbContext
        private readonly IServiceProvider _serviceProvider;
        // Memory cache instance for storing cached client data in-memory
        private readonly IMemoryCache _memoryCache;
        // Constructor injects required services: IServiceProvider and IMemoryCache
        public ClientCacheService(IServiceProvider serviceProvider, IMemoryCache memoryCache)
        {
            _serviceProvider = serviceProvider;
            _memoryCache = memoryCache;
        }
        // Gets a Client entity by ClientId asynchronously.
        // First attempts to get the client from in-memory cache.
        // If the client is not found in cache, fetches from the database,
        // caches the client, then returns it.
        public async Task<ClientSecret?> GetClientByClientIdAsync(string clientCode)
        {
            // Construct cache key for this client using prefix and clientId
            var cacheKey = CacheKeyPrefix + clientCode;
            // Attempt to retrieve the client from in-memory cache
            if (_memoryCache.TryGetValue<ClientSecret>(cacheKey, out var client))
            {
                // Cache hit - return the cached client immediately
                return client;
            }
            // Cache miss - create a new scope to get a fresh DbContext instance
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            // Query the database asynchronously for active client matching the clientId
            client = await dbContext.ClientSecrets.AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClientId == clientCode);
            if (client != null)
            {
                // Store the retrieved client into cache with expiration policy
                // Here it expires 1 hour after being added to cache; adjust as needed
                _memoryCache.Set(cacheKey, client, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });
            }
            // Return the client entity (null if not found in DB)
            return client;
        }
    }
}
