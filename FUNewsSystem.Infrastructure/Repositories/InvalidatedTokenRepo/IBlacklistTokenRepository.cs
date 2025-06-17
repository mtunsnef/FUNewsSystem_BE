using FUNewsSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Infrastructure.Repositories.InvalidatedTokenRepo
{
    public interface IBlacklistTokenRepository : IRepositoryBase<InvalidatedToken>
    {
        Task<bool> IsBlacklistedAsync(string token);
        Task AddAsync(string token, DateTime expiryTime);
        Task CleanupExpiredAsync();
    }
}
