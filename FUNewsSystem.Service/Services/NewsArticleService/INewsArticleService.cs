using FUNewsSystem.Domain.Models;
using FUNewsSystem.Service.DTOs.NewsArticleDto;
using FUNewsSystem.Service.DTOs.ResponseDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.NewsArticleService
{
    public interface INewsArticleService
    {
        Task<NewsArticle> GetByIdAsync(string id);
        Task<ApiResponseDto<string>> CreateNewsArticle(CreateUpdateNewsArticleDto dto);
        Task<ApiResponseDto<string>> UpdateNewsArticle(string id, CreateUpdateNewsArticleDto dto);
        Task<ApiResponseDto<string>> DeleteNewsArticle(string id);
        Task<List<NewsArticle>> GetAllAsync();
        IQueryable<NewsArticle> GetQueryable();
        Task<ApiResponseDto<string>> PostNewsArticleAsync(SystemAccount user, PostNewsArticleforUserDto dto);
        Task<List<PostManageDto>> GetArticlesByStatusAsync(string status, string userId);

    }
}
