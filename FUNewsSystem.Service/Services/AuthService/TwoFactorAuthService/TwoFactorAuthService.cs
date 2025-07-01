using FUNewsSystem.Infrastructure.Repositories.SystemAccountRepo;
using FUNewsSystem.Service.DTOs.AuthDto.TwoFaDto;
using FUNewsSystem.Service.Services.AuthService.AzureRedisTokenStoreService;
using FUNewsSystem.Service.Services.ConfigService;
using FUNewsSystem.Service.Services.SystemAccountService;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FUNewsSystem.Service.Services.AuthService.TwoFactorAuthService
{
    public class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly ISystemAccountRepository _systemAccountRepository;
        private readonly IAuthService _authService;
        private readonly IRefreshTokenStoreSerivce _tokenStoreService;
        private readonly IConfigService _configService;


        public TwoFactorAuthService(IConfigService configService, IRefreshTokenStoreSerivce tokenStoreService, ISystemAccountRepository systemAccountRepository, IAuthService authService)
        {
            _systemAccountRepository = systemAccountRepository;
            _authService = authService;
            _tokenStoreService = tokenStoreService;
            _configService = configService;
        }
        public async Task<TwoFaInitDto?> GenerateSecretAsync(string userId)
        {
            var user = await _systemAccountRepository.GetByIdAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            var secretKey = KeyGeneration.GenerateRandomKey(20);
            var base32Secret = Base32Encoding.ToString(secretKey);

            user.Temp2FASecretKey = base32Secret;
            await _systemAccountRepository.UpdateAsync(user);

            string issuer = "FU News";
            string email = user.AccountEmail;
            string qrCodeUri = $"otpauth://totp/{HttpUtility.UrlEncode(issuer)}:{HttpUtility.UrlEncode(email)}?secret={base32Secret}&issuer={HttpUtility.UrlEncode(issuer)}&digits=6";

            return new TwoFaInitDto
            {
                SharedKey = base32Secret,
                QrCodeUri = qrCodeUri
            };
        }

        public async Task<bool> VerifyCodeAsync(string userId, string code)
        {
            var user = await _systemAccountRepository.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Temp2FASecretKey))
                throw new UnauthorizedAccessException("User not found or no temp key");

            var totp = new Totp(Base32Encoding.ToBytes(user.Temp2FASecretKey));
            bool isValid = totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

            if (!isValid)
                return false;

            user.Is2FAEnabled = true;
            user.TwoFactorSecretKey = user.Temp2FASecretKey;
            user.Temp2FASecretKey = null;
            await _systemAccountRepository.UpdateAsync(user);

            return true;
        }

        public async Task<bool> Check2FAEnabledAsync(string userId)
        {
            var user = await _systemAccountRepository.GetByIdAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            return user.Is2FAEnabled;
        }

        public async Task<bool> VerifyCodeAfterLoginAsync(string userId, string code)
        {
            var user = await _systemAccountRepository.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecretKey))
                throw new UnauthorizedAccessException("No 2FA secret found");

            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecretKey));
            var isValid = totp.VerifyTotp(code, out _);

            if (!isValid) return false;

            var tokenPayload = _authService.GenerateTokenPayload(user);
            var refreshKey = $"{user.AccountId}";
            var refreshTTL = TimeSpan.FromSeconds(_configService.GetInt("Jwt:Lifetime:RefreshToken"));
            await _tokenStoreService.SetAsync(refreshKey, tokenPayload.RefreshToken, refreshTTL);

            return true;
        }


    }
}
