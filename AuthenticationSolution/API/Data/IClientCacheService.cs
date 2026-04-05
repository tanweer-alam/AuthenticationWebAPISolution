using API.Models;

namespace API.Data
{
    public interface IClientCacheService
    {
        // Async method: fetch from cache or DB if missing and update cache
        Task<ClientSecret?> GetClientByClientIdAsync(string clientId);
    }
}
