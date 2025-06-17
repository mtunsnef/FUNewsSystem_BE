using FUNewsSystem.Infrastructure.Repositories.InvalidatedTokenRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.AuthService
{
    public class BlacklistTokenService : IBlacklistTokenService
    {
        private readonly IBlacklistTokenRepository _blacklistTokenRepository;
        public BlacklistTokenService(IBlacklistTokenRepository blacklistTokenRepository)
        {
            _blacklistTokenRepository = blacklistTokenRepository;
        }

        public async Task AddAsync(string token, DateTime expiryTime)
        {
            await _blacklistTokenRepository.AddAsync(token, expiryTime);
        }

        public async Task CleanupExpiredAsync()
        {
            await _blacklistTokenRepository.CleanupExpiredAsync();
        }

        public async Task<bool> IsBlacklistedAsync(string token)
        {
            return await _blacklistTokenRepository.IsBlacklistedAsync(token);
        }
    }
}
