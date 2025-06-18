using FUNewsSystem.Domain.Consts;
using FUNewsSystem.Domain.Enums.Auth;
using FUNewsSystem.Domain.Exceptions.Http;
using FUNewsSystem.Domain.Models;
using FUNewsSystem.Infrastructure.Repositories.SystemAccountRepo;
using FUNewsSystem.Service.DTOs.AuthDto;
using FUNewsSystem.Service.DTOs.AuthDto.ExternalDto;
using FUNewsSystem.Service.DTOs.ResponseDto;
using FUNewsSystem.Service.DTOs.SystemAccountDto;
using FUNewsSystem.Service.Services.AuthService;
using FUNewsSystem.Service.Services.ConfigService;
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
        public AuthController(IAuthService authService, IConfigService configService)
        {
            _authService = authService;
            _configService = configService;
        }

        [HttpPost("token")]
        public async Task<ActionResult> Login([FromBody] UserCredentialDto dto)
        {
            var payload = await _authService.Login(dto);
            var refreshTokenLifetimeSeconds = _configService.GetInt("Jwt:Lifetime:RefreshToken");

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddSeconds(refreshTokenLifetimeSeconds)
            };

            Response.Cookies.Append("refresh_token", payload.AccessToken.RefreshToken, cookieOptions);

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
        public async Task<ActionResult<TokenPayloadDto>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refresh_token"];

            var payload = await _authService.RefreshTokenAsync(refreshToken); 
            var refreshTokenLifetimeSeconds = _configService.GetInt("Jwt:Lifetime:RefreshToken");

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddSeconds(refreshTokenLifetimeSeconds)
            };
            Response.Cookies.Append("refresh_token", payload.RefreshToken, cookieOptions);

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
            var redirectUrl = Url.Action("ExternalLoginCallback", "Auth", new { returnUrl, provider });
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
                : Redirect($"{returnUrl}?token={payload.AccessToken}");
        }

        [HttpPost("complete-register")]
        public async Task<IActionResult> CompleteExternalRegister([FromBody] CompleteExternalRegisterDto dto)
        {
            var result = await _authService.CompleteRegisterAsync(dto);
            return Ok(result);
        }

    }
}
