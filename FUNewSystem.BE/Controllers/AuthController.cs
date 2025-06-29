using FUNewsSystem.Domain.Consts;
using FUNewsSystem.Domain.Enums.Auth;
using FUNewsSystem.Domain.Exceptions.Http;
using FUNewsSystem.Domain.Models;
using FUNewsSystem.Infrastructure.Repositories.SystemAccountRepo;
using FUNewsSystem.Service.DTOs.AuthDto;
using FUNewsSystem.Service.DTOs.AuthDto.ExternalDto;
using FUNewsSystem.Service.DTOs.AuthDto.TwoFaDto;
using FUNewsSystem.Service.DTOs.ResponseDto;
using FUNewsSystem.Service.DTOs.SystemAccountDto;
using FUNewsSystem.Service.Services.AuthService;
using FUNewsSystem.Service.Services.AuthService.TwoFactorAuthService;
using FUNewsSystem.Service.Services.ConfigService;
using FUNewsSystem.Service.Services.HttpContextService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.CodeDom;
using System.Net;
using System.Security.Claims;

namespace FUNewSystem.BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfigService _configService;
        private readonly ITwoFactorAuthService _twoFactorAuthService;
        private readonly IHttpContextService _httpContextService;

        public AuthController(IHttpContextService httpContextService, IAuthService authService, IConfigService configService, ITwoFactorAuthService twoFactorAuthService)
        {
            _authService = authService;
            _configService = configService;
            _twoFactorAuthService = twoFactorAuthService;
            _httpContextService = httpContextService;
        }

        [HttpPost("token")]
        public async Task<ActionResult> Login([FromBody] UserCredentialDto dto)
        {
            var payload = await _authService.Login(dto);
            return Ok(new
            {
                accessToken = payload.AccessToken.AccessToken,
                refreshToken = payload.AccessToken.RefreshToken,
                authenticated = payload.Authenticated
            });
        }

        [HttpGet("myinfo")]
        [Authorize]
        public async Task<ActionResult<SystemAccount>> GetMyInfo()
        {
            return Ok(await _authService.GetMe());
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            var payload = await _authService.RefreshTokenAsync(dto.AccessToken);

            return Ok(new
            {
                accessToken = payload.AccessToken,
                refreshToken = payload.RefreshToken,
                expiresIn = payload.ExpiresIn
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutRequestDto dto)
        {
            await _authService.LogoutAsync(dto);
            return Ok(new { message = "Logged out successfully." });
        }


        [HttpGet("external-login")]
        public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Auth", new { returnUrl, provider }, Request.Scheme, Request.Host.Value);
            var props = new AuthenticationProperties { RedirectUri = redirectUrl };

            return Challenge(props, provider switch
            {
                "Google" => AuthSchemes.Google,
                "Facebook" => AuthSchemes.Facebook,
                _ => throw new ArgumentException("Provider không hợp lệ", nameof(provider))
            });
        }

        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback([FromQuery] string returnUrl, [FromQuery] string provider)
        {
            var scheme = provider.ToLower() switch
            {
                AuthProviders.Google => AuthSchemes.Google,
                AuthProviders.Facebook => AuthSchemes.Facebook,
                _ => throw new NotSupportedException("Provider không hợp lệ.")
            };

            var result = await HttpContext.AuthenticateAsync(scheme);
            if (!result.Succeeded)
                return Redirect($"{returnUrl}?error=auth_failed");

            var principal = result.Principal;

            var externalId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(externalId))
                return Redirect($"{returnUrl}?error=missing_info");

            var payload = await _authService.HandleExternalLoginAsync(email, name, externalId);

            return payload.IsNewAccount
                ? Redirect($"{returnUrl}?register=true&email={email}&name={name}&externalId={externalId}&provider={provider}")
                : Redirect($"{returnUrl}?token={payload.AccessToken}&is2FAEnabled={payload.Is2FAEnabled.ToString().ToLower()}");
        }

        [HttpPost("complete-register")]
        public async Task<IActionResult> CompleteExternalRegister([FromBody] CompleteExternalRegisterDto dto)
        {
            var payload = await _authService.CompleteRegisterAsync(dto);
            return Ok(new
            {
                accessToken = payload.AccessToken.AccessToken,
                refreshToken = payload.AccessToken.RefreshToken,
                authenticated = payload.Authenticated
            });
        }

        [HttpGet("2fa/generate-secret")]
        public async Task<ActionResult> GenerateSecret()
        {
            var systemAccount = await _httpContextService.GetSystemAccountAndThrow();
            if (string.IsNullOrEmpty(systemAccount.AccountId))
                return Unauthorized();

            try
            {
                var result = await _twoFactorAuthService.GenerateSecretAsync(systemAccount.AccountId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi tạo mã 2FA.");
            }
        }


        [HttpPost("2fa/verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] TwoFaVerifyDto dto)
        {
            var systemAccount = await _httpContextService.GetSystemAccountAndThrow();
            if (string.IsNullOrEmpty(systemAccount.AccountId))
                return Unauthorized();

            try
            {
                var isValid = await _twoFactorAuthService.VerifyCodeAsync(systemAccount.AccountId, dto.Code);
                if (!isValid)
                    return BadRequest(new { message = "Mã xác thực không đúng." });

                return Ok(new { message = "Xác thực thành công. 2FA đã được bật." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi xác minh mã 2FA.");
            }
        }

        [HttpGet("2fa/is-enabled")]
        public async Task<IActionResult> Is2FAEnabled()
        {
            var systemAccount = await _httpContextService.GetSystemAccountAndThrow();
            if (string.IsNullOrEmpty(systemAccount.AccountId))
                return Unauthorized();

            bool isEnabled = await _twoFactorAuthService.Check2FAEnabledAsync(systemAccount.AccountId);
            return Ok(new { is2FAEnabled = isEnabled });
        }


        [HttpPost("2fa/verify-token-after-login")]
        [Authorize]
        public async Task<IActionResult> VerifyTokenAfterLogin([FromBody] TwoFaCodeDto dto)
        {
            var systemAccount = await _httpContextService.GetSystemAccountAndThrow();
            if (string.IsNullOrEmpty(systemAccount.AccountId))
                return Unauthorized();

            try
            {
                var isValid = await _twoFactorAuthService.VerifyCodeAfterLoginAsync(systemAccount.AccountId, dto.Code);

                if (!isValid)
                    return BadRequest(new { message = "Mã xác thực không đúng." });

                return Ok(new { message = "Xác thực thành công." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi xác minh mã 2FA." });
            }
        }

    }
}
