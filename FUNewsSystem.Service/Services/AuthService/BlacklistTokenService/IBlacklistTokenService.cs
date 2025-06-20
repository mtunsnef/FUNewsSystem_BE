using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.AuthService.BlacklistTokenService
{
    public interface IBlacklistTokenService
    {
        Task<bool> IsBlacklistedAsync(string jti);
        Task BlacklistAsync(string jti, DateTime expiresAt);
    }
}
