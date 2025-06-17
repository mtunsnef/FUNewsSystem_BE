using FUNewsSystem.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Infrastructure.Repositories.InvalidatedTokenRepo
{
    public class BlacklistTokenRepository : RepositoryBase<InvalidatedToken>, IBlacklistTokenRepository
    {
        public BlacklistTokenRepository(FunewsSystemApiDbContext context) : base(context)
        {
        }

        public async Task<bool> IsBlacklistedAsync(string token)
        {
            return await _dbSet
                .AnyAsync(t => t.Token == token && t.ExpiryTime > DateTime.UtcNow);
        }

        public async Task AddAsync(string token, DateTime expiryTime)
        {
            var entity = new InvalidatedToken
            {
                Token = token,
                ExpiryTime = expiryTime
            };

            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task CleanupExpiredAsync()
        {
            var expiredTokens = await _dbSet
                .Where(t => t.ExpiryTime <= DateTime.UtcNow)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                _dbSet.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
            }
        }
    }
}
