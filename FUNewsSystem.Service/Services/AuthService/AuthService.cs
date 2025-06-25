using FUNewsSystem.Domain.Enums.Auth;
using FUNewsSystem.Domain.Exceptions.Http;
using FUNewsSystem.Domain.Extensions.SystemAccounts;
using FUNewsSystem.Domain.Models;
using FUNewsSystem.Infrastructure.Repositories.SystemAccountRepo;
using FUNewsSystem.Service.DTOs.AuthDto;
using FUNewsSystem.Service.DTOs.AuthDto.ExternalDto;
using FUNewsSystem.Service.DTOs.ResponseDto;
using FUNewsSystem.Service.DTOs.SystemAccountDto;
using FUNewsSystem.Service.Services.AuthService.AzureRedisTokenStoreService;
using FUNewsSystem.Service.Services.AuthService.BlacklistTokenService;
using FUNewsSystem.Service.Services.ConfigService;
using FUNewsSystem.Service.Services.HttpContextService;
using FUNewsSystem.Service.Services.SystemAccountService;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using BC = BCrypt.Net.BCrypt;

namespace FUNewsSystem.Service.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IConfigService _configService;
        private readonly IHttpContextService _httpContextService;
        private readonly ISystemAccountService _systemAccountService;
        private readonly IBlacklistTokenService _blacklistTokenService;
        private readonly IRefreshTokenStoreSerivce _tokenStoreService;
        private readonly ISystemAccountRepository _systemAccountRepository;
        public AuthService(IRefreshTokenStoreSerivce tokenStoreService, ISystemAccountRepository systemAccountRepository, IConfigService configService, ISystemAccountService systemAccountService, IHttpContextService httpContextService, IBlacklistTokenService blacklistTokenService)
        {
            _configService = configService;
            _httpContextService = httpContextService;
            _systemAccountService = systemAccountService;
            _blacklistTokenService = blacklistTokenService;
            _systemAccountRepository = systemAccountRepository;
            _tokenStoreService = tokenStoreService;
        }

        public async Task<LoginPayloadDto> Login(UserCredentialDto dto)
        {
            var account = await _systemAccountService.GetAccountByEmail(dto.Email);

            if (account is null || !BC.Verify(dto.Password, account.AccountPassword))
                throw new UnauthorizedException("Username or Password is not correct.");

            var tokenPayload = GenerateTokenPayload(account);

            var refreshKey = $"{account.AccountId}";
            var refreshTTL = TimeSpan.FromSeconds(_configService.GetInt("Jwt:Lifetime:RefreshToken"));
            await _tokenStoreService.SetAsync(refreshKey, tokenPayload.RefreshToken, refreshTTL);

            return new LoginPayloadDto
            {
                AccessToken = tokenPayload,
                Authenticated = true
            };
        }

        public async Task LogoutAsync(LogoutRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                throw new UnauthorizedException("Token is missing");

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(request.Token))
                throw new UnauthorizedException("Invalid token format");

            var jwt = handler.ReadJwtToken(request.Token);
            var jti = jwt.Id;
            var userId = jwt.Claims.First(c => c.Type == "AccountId").Value;
            var exp = jwt.ValidTo;

            await _blacklistTokenService.BlacklistAsync(jti, exp);
            await _tokenStoreService.DeleteAsync(userId);
        }

        public async Task<SystemAccount> GetMe()
        {
            return await _httpContextService.GetSystemAccountAndThrow();
        }


        private DateTime GetTokenExpiry(string token)
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.ValidTo;
        }

        private TokenPayloadDto GenerateTokenPayload(SystemAccount systemAccount)
        {
            var accessToken = GenerateToken(systemAccount, AuthToken.AccessToken);
            var refreshToken = GenerateToken(systemAccount, AuthToken.RefreshToken);

            return new TokenPayloadDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _configService.GetInt("Jwt:Lifetime:AccessToken")
            };
        }

        private ClaimsPrincipal ValidateToken(string token, string secret)
        {
            var handler = new JwtSecurityTokenHandler();

            handler.InboundClaimTypeMap.Clear();
            handler.MapInboundClaims = false;

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                RequireExpirationTime = false,
                IssuerSigningKey = new SymmetricSecurityKey(
                                     Encoding.UTF8.GetBytes(secret))
            };

            return handler.ValidateToken(token, parameters, out _);
        }

        private ClaimsPrincipal ValidateRefreshToken(string token)
        {
            var principal = ValidateToken(
                token,
                _configService.GetString("Jwt:SecretKey")
            );
           
            return principal;
        }

        public async Task<TokenPayloadDto> RefreshTokenAsync(string currentAccessToken)
        {
            var accessJwt = new JwtSecurityTokenHandler().ReadJwtToken(currentAccessToken);
            var userId = accessJwt.Claims.FirstOrDefault(c => c.Type == "AccountId")?.Value
                         ?? throw new UnauthorizedException("Invalid user ID");

            var storedRefreshToken = await _tokenStoreService.GetAsync(userId)
                                       ?? throw new UnauthorizedException("Refresh token missing");

            var principal = ValidateRefreshToken(storedRefreshToken);

            var oldRefreshJti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var oldRefreshExp = GetTokenExpiry(storedRefreshToken);

            var accJti = accessJwt.Id;
            var accExp = GetTokenExpiry(currentAccessToken);

            if (await _blacklistTokenService.IsBlacklistedAsync(accJti))
                throw new UnauthorizedException("Access token revoked");

            if (await _blacklistTokenService.IsBlacklistedAsync(
                new JwtSecurityTokenHandler().ReadJwtToken(storedRefreshToken).Id))

                        throw new UnauthorizedException("Refresh token revoked");
            await _blacklistTokenService.BlacklistAsync(accJti, accExp);
            await _blacklistTokenService.BlacklistAsync(oldRefreshJti, oldRefreshExp);
            await _tokenStoreService.DeleteAsync(userId);

            var account = await _systemAccountService.GetUserByIdAsync(userId)
                           ?? throw new UnauthorizedException("User not found");

            var newPayload = GenerateTokenPayload(account);
            var ttl = TimeSpan.FromSeconds(_configService.GetInt("Jwt:Lifetime:RefreshToken"));

            await _tokenStoreService.SetAsync(userId, newPayload.RefreshToken, ttl);

            return newPayload;
        }


        private string GenerateToken(SystemAccount acc, AuthToken type)
        {
            var secret = _configService.GetString("Jwt:SecretKey")!.Trim();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var life = type == AuthToken.AccessToken
                            ? _configService.GetInt("Jwt:Lifetime:AccessToken")
                            : _configService.GetInt("Jwt:Lifetime:RefreshToken");

            var tokenDesc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, _configService.GetString("Jwt:Subject")),
                    new Claim("AccountId", acc.AccountId.ToString()),
                    new Claim("Email",     acc.AccountEmail ?? ""),
                    new Claim(ClaimTypes.Role, acc.GetRoleConst() ?? ""),
                    new Claim("token_type", type == AuthToken.AccessToken ? "access" : "refresh")
                }),
                Expires = DateTime.UtcNow.AddSeconds(life),
                Issuer = _configService.GetString("Jwt:Issuer"),
                Audience = _configService.GetString("Jwt:Audience"),
                SigningCredentials = creds
            };

            return new JwtSecurityTokenHandler().WriteToken(
                    new JwtSecurityTokenHandler().CreateToken(tokenDesc));
        }

        public async Task<ExternalLoginPayloadDto> HandleExternalLoginAsync(string email, string? name, string externalId)
        {
            var account = await _systemAccountService.GetAccountByEmail(email);

            if (account == null)
            {
                return new ExternalLoginPayloadDto
                {
                    IsNewAccount = true
                };
            }

            var token = GenerateToken(account, AuthToken.AccessToken);

            return new ExternalLoginPayloadDto
            {
                IsNewAccount = false,
                AccessToken = token,
                Is2FAEnabled = account.Is2FAEnabled
            };
        }


        public async Task<LoginPayloadDto> CompleteRegisterAsync(CompleteExternalRegisterDto dto)
        {
            var existingAccount = await _systemAccountRepository.GetAccountByEmail(dto.Email);

            if (existingAccount != null)
            {
                throw new ConflictException("Email đã tồn tại");
            }

            var account = new SystemAccount
            {
                AccountId = Guid.NewGuid().ToString(),
                AccountName = dto.Name,
                AccountEmail = dto.Email,
                AuthProvider = dto.Provider,
                AuthProviderId = dto.ExternalId,
                AccountRole = 2,
                PhoneNumber = dto.PhoneNumber
            };

            await _systemAccountRepository.AddAsync(account);

            var tokenPayload = GenerateTokenPayload(account);
            var refreshKey = $"{account.AccountId}";
            var refreshTTL = TimeSpan.FromSeconds(_configService.GetInt("Jwt:Lifetime:RefreshToken"));
            await _tokenStoreService.SetAsync(refreshKey, tokenPayload.RefreshToken, refreshTTL);

            return new LoginPayloadDto
            {
                AccessToken = tokenPayload,
                Authenticated = true
            };
        }
    }
}
