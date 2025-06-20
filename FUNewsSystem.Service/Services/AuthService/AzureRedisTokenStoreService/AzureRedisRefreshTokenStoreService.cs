using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.AuthService.AzureRedisTokenStoreService
{
    public class AzureRedisRefreshTokenStoreService : IRefreshTokenStoreSerivce
    {
        private readonly IDatabase _db;
        public AzureRedisRefreshTokenStoreService(IConnectionMultiplexer redis)
            => _db = redis.GetDatabase();

        private static string Key(string userId) => $"refresh:{userId}";

        public Task SetAsync(string userId, string token, TimeSpan ttl)
            => _db.StringSetAsync(Key(userId), token, ttl);

        public async Task<string?> GetAsync(string userId)
        {
            var val = await _db.StringGetAsync(Key(userId));
            return val.IsNull ? null : val.ToString();
        }

        public Task DeleteAsync(string userId)
            => _db.KeyDeleteAsync(Key(userId));
    }
}
