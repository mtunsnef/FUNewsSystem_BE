using AutoMapper;
using FUNewsSystem.Domain.Exceptions.Http;
using FUNewsSystem.Domain.Models;
using FUNewsSystem.Infrastructure.Repositories.SystemAccountRepo;
using FUNewsSystem.Service.DTOs.AuthDto.ExternalDto;
using FUNewsSystem.Service.DTOs.ResponseDto;
using FUNewsSystem.Service.DTOs.SystemAccountDto;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.SystemAccountService
{
    public class SystemAccountService : ISystemAccountService
    {
        private readonly ISystemAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        public SystemAccountService(ISystemAccountRepository accountRepository, IMapper mapper)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
        }

        public Task<List<SystemAccount>> GetAllAsync()
        {
            return _accountRepository.GetAllAsync();
        }

        public async Task<ApiResponseDto<string>> CreateAccount(CreateUpdateSystemAccountDto dto)
        {
            var allAccount = await _accountRepository.GetAllAsync();
            bool exists = allAccount.Any(c => c.AccountEmail == dto.Email);

            if (exists)
                throw new ConflictException("Email is already in use.");

            var account = _mapper.Map<SystemAccount>(dto);
            await _accountRepository.AddAsync(account);

            return ApiResponseDto<string>.SuccessResponse("Account created successfully.");
        }

        public async Task<ApiResponseDto<string>> UpdateAccount(string id, CreateUpdateSystemAccountDto dto)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
                throw new NotFoundException($"SystemAccount with ID {id} not found.");

            _mapper.Map(dto, account);
            await _accountRepository.UpdateAsync(account);

            return ApiResponseDto<string>.SuccessResponse("Account updated successfully.");
        }

        public async Task<ApiResponseDto<string>> DeleteAccount(string id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
                throw new NotFoundException($"SystemAccount with ID {id} not found.");

            await _accountRepository.DeleteAsync(account);

            return ApiResponseDto<string>.SuccessResponse("Account deleted successfully.");
        }

        public async Task<SystemAccount?> GetUserByIdAsync(string id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
                throw new NotFoundException($"SystemAccount with ID {id} not found.");

            return account;
        }

        public async Task<SystemAccount?> GetAccountByEmail(string email)
        {
            return await _accountRepository.GetAccountByEmail(email);
        }

        public async Task<SystemAccount> CreateExternalAccountAsync(CreateSystemAccountDto dto)
        {
            var existing = await _accountRepository.GetAccountByEmail(dto.AccountEmail);

            if (existing != null)
                throw new ConflictException("Email already in use");

            var account = new SystemAccount
            {
                AccountEmail = dto.AccountEmail,
                AuthProvider = dto.AuthProvider.ToLower(),
                AuthProviderId = dto.AuthProviderId,
                AccountRole = 2,
                AccountPassword = null
            };

            await _accountRepository.AddAsync(account);
            return account;
        }

    }
}
