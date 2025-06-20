using FUNewsSystem.Infrastructure.Repositories.InvalidatedTokenRepo;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.AuthService.BlacklistTokenService
{
    public class BlacklistTokenService : IBlacklistTokenService
    {
        private readonly IDatabase _db;
        public BlacklistTokenService(IConnectionMultiplexer redis) => _db = redis.GetDatabase();

        public Task<bool> IsBlacklistedAsync(string jti)
            => _db.KeyExistsAsync($"blacklist:{jti}");

        public async Task BlacklistAsync(string jti, DateTime expiresAt)
        {
            var ttl = expiresAt.ToUniversalTime() - DateTime.UtcNow;

            if (ttl <= TimeSpan.Zero)
                ttl = TimeSpan.FromSeconds(1000);

            await _db.StringSetAsync($"blacklist:{jti}", "revoked", ttl);
        }

    }
}
