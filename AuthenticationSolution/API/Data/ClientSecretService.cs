using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class ClientSecretService
    {
        private readonly UserDbContext _dbContext;
        public ClientSecretService(UserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        public async Task<string?> GetClientSecretKeyAsync(string clientId) 
        {
            var client = await _dbContext.ClientSecrets.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == clientId.Trim());
            return client?.SecretKey;
        }
    }
}
