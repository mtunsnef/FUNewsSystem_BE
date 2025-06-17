using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.AuthService
{
    public interface IBlacklistTokenService
    {
        Task<bool> IsBlacklistedAsync(string token);
        Task AddAsync(string token, DateTime expiryTime);
        Task CleanupExpiredAsync();
    }
}
