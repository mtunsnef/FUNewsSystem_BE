using FUNewsSystem.Service.DTOs.AuthDto.TwoFaDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.AuthService.TwoFactorAuthService
{
    public interface ITwoFactorAuthService
    {
        Task<TwoFaInitDto?> GenerateSecretAsync(string userId);
        Task<bool> VerifyCodeAsync(string userId, string code);
        Task<bool> Check2FAEnabledAsync(string userId);
        Task<bool> VerifyCodeAfterLoginAsync(string userId, string code);
    }

}
