using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.AuthService.AzureRedisTokenStoreService
{
    public interface IRefreshTokenStoreSerivce
    {
        Task SetAsync(string userId, string refreshToken, TimeSpan ttl);
        Task<string?> GetAsync(string userId);
        Task DeleteAsync(string userId);
    }
}
