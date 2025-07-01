using FUNewsSystem.Domain.Models;
using FUNewsSystem.Service.DTOs.AuthDto;
using FUNewsSystem.Service.DTOs.AuthDto.ExternalDto;
using FUNewsSystem.Service.DTOs.ResponseDto;
using FUNewsSystem.Service.DTOs.SystemAccountDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.AuthService
{
    public interface IAuthService
    {
        Task<SystemAccount> GetMe();
        Task LogoutAsync(LogoutRequestDto request);
        Task<TokenPayloadDto> RefreshTokenAsync(string currentAccessToken);
        Task<LoginPayloadDto> Login(UserCredentialDto dto);
        Task<ExternalLoginPayloadDto> HandleExternalLoginAsync(string email, string? name, string externalId);
        Task<LoginPayloadDto> CompleteRegisterAsync(CompleteExternalRegisterDto dto);
        Task<RegisterPayloadDto> RegisterAccountAsync(RegisterRequestDto request);
        TokenPayloadDto GenerateTokenPayload(SystemAccount systemAccount);
    }
}
