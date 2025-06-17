using FUNewsSystem.Domain.Enums.Auth;
using FUNewsSystem.Domain.Exceptions.Http;
using FUNewsSystem.Domain.Extensions.SystemAccounts;
using FUNewsSystem.Domain.Models;
using FUNewsSystem.Infrastructure.Repositories.SystemAccountRepo;
using FUNewsSystem.Service.DTOs.AuthDto;
using FUNewsSystem.Service.DTOs.AuthDto.ExternalDto;
using FUNewsSystem.Service.DTOs.ResponseDto;
using FUNewsSystem.Service.DTOs.SystemAccountDto;
using FUNewsSystem.Service.Services.ConfigService;
using FUNewsSystem.Service.Services.HttpContextService;
using FUNewsSystem.Service.Services.SystemAccountService;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
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
        private readonly ISystemAccountRepository _systemAccountRepository;
        public AuthService(ISystemAccountRepository systemAccountRepository, IConfigService configService, ISystemAccountService systemAccountService, IHttpContextService httpContextService, IBlacklistTokenService blacklistTokenService)
        {
            _configService = configService;
            _httpContextService = httpContextService;
            _systemAccountService = systemAccountService;
            _blacklistTokenService = blacklistTokenService;
            _systemAccountRepository = systemAccountRepository;
        }

        public async Task<LoginPayloadDto> Login(UserCredentialDto dto)
        {
            var account = await _systemAccountService.GetAccountByEmail(dto.Email);

            if (account is null || !BC.Verify(dto.Password, account.AccountPassword))
            {
                throw new UnauthorizedException("Username or Password is not correct.");
            }

            return new LoginPayloadDto
            {
                AccessToken = GenerateTokenPayload(account),
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

            if (string.IsNullOrEmpty(jti))
                throw new UnauthorizedException("Invalid token JTI");

            var expiry = jwt.ValidTo;

            if (expiry < DateTime.UtcNow)
                throw new UnauthorizedException("Token already expired");

            await _blacklistTokenService.AddAsync(jti, expiry);
        }


        public async Task<SystemAccount> GetMe()
        {
            return await _httpContextService.GetSystemAccountAndThrow();
        }
        private ClaimsPrincipal? ValidateToken(string token, string secret)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };

                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch
            {
                return null;
            }
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

        public async Task<TokenPayloadDto> RefreshTokenAsync(string token)
        {
            var principal = ValidateToken(
                token, _configService.GetString("Jwt:SecretKey"));

            var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value ?? throw new UnauthorizedException("Invalid jti");

            if (await _blacklistTokenService.IsBlacklistedAsync(jti)) throw new UnauthorizedException("Token already used");

            var expiry = GetTokenExpiry(token);
            await _blacklistTokenService.AddAsync(jti, expiry);

            var accId = principal.FindFirst("AccountId")!.Value;
            var account = await _systemAccountService.GetUserByIdAsync(accId) ?? throw new UnauthorizedException("User not found");

            var newAccess = GenerateToken(account, AuthToken.AccessToken);
            var newRefresh = GenerateToken(account, AuthToken.RefreshToken);

            return new TokenPayloadDto
            {
                AccessToken = newAccess,
                RefreshToken = newRefresh,
                ExpiresIn = _configService.GetInt("Jwt:Lifetime:AccessToken")
            };
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
                    new Claim(ClaimTypes.Role, acc.GetRoleConst() ?? "")
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
                AccessToken = token
            };
        }

        public async Task<ApiResponseDto<string>> CompleteRegisterAsync(CompleteExternalRegisterDto dto)
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

            var token = GenerateToken(account, AuthToken.AccessToken);
            return ApiResponseDto<string>.SuccessResponse(token);
        }
    }
}
