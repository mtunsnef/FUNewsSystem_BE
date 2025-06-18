using FUNewsSystem.Domain.Models;
using FUNewsSystem.Service.DTOs.AuthDto.ExternalDto;
using FUNewsSystem.Service.DTOs.ResponseDto;
using FUNewsSystem.Service.DTOs.SystemAccountDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.SystemAccountService
{
    public interface ISystemAccountService
    {
        Task<SystemAccount?> GetUserByIdAsync(string id);
        Task<List<SystemAccount>> GetAllAsync();
        Task<ApiResponseDto<string>> CreateAccount(CreateUpdateSystemAccountDto dto);
        Task<ApiResponseDto<string>> UpdateAccount(string id, CreateUpdateSystemAccountDto dto);
        Task<ApiResponseDto<string>> DeleteAccount(string id);
        Task<SystemAccount?> GetAccountByEmail(string email);
        Task<SystemAccount> CreateExternalAccountAsync(CreateSystemAccountDto dto);
    }
}
