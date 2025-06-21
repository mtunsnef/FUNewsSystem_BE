using AutoMapper;
using FUNewsSystem.Domain.Exceptions.Http;
using FUNewsSystem.Domain.Models;
using FUNewsSystem.Infrastructure.Repositories.NewsArticleRepo;
using FUNewsSystem.Infrastructure.Repositories.TagRepo;
using FUNewsSystem.Service.DTOs.NewsArticleDto;
using FUNewsSystem.Service.DTOs.ResponseDto;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.NewsArticleService
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly INewsArticleRepository _newsArticleRepository;
        private readonly IMapper _mapper;
        private readonly ITagRepository _tagRepository;
        public NewsArticleService(INewsArticleRepository newsArticleRepository, IMapper mapper, ITagRepository tagRepository)
        {
            _newsArticleRepository = newsArticleRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
        }

        public Task<List<NewsArticle>> GetAllAsync()
        {
            return _newsArticleRepository.GetAllAsync();
        }
        public IQueryable<NewsArticle> GetQueryable()
        {
            return _newsArticleRepository.GetQueryable();
        }
        public async Task<NewsArticle> GetByIdAsync(string id)
        {
            var article = await _newsArticleRepository.GetByIdAsync(id);
            if (article == null)
                throw new NotFoundException($"NewsArticle with ID {id} not found.");

            return article;
        }

        public async Task<ApiResponseDto<string>> CreateNewsArticle(CreateUpdateNewsArticleDto dto)
        {
            var article = _mapper.Map<NewsArticle>(dto);
            await _newsArticleRepository.AddAsync(article);

            return ApiResponseDto<string>.SuccessResponse("NewsArticle created successfully.");
        }

        public async Task<ApiResponseDto<string>> UpdateNewsArticle(string id, CreateUpdateNewsArticleDto dto)
        {
            var article = await _newsArticleRepository.GetByIdAsync(id);
            if (article == null)
                throw new NotFoundException($"NewsArticle with ID {id} not found.");

            _mapper.Map(dto, article);
            await _newsArticleRepository.UpdateAsync(article);

            return ApiResponseDto<string>.SuccessResponse("NewsArticle updated successfully.");
        }

        public async Task<ApiResponseDto<string>> DeleteNewsArticle(string id)
        {
            var article = await _newsArticleRepository.GetByIdAsync(id);
            if (article == null)
                throw new NotFoundException($"NewsArticle with ID {id} not found.");

            await _newsArticleRepository.DeleteAsync(article);

            return ApiResponseDto<string>.SuccessResponse("NewsArticle deleted successfully.");
        }

            public async Task<ApiResponseDto<string>> PostNewsArticleAsync(SystemAccount user, PostNewsArticleforUserDto dto)
            {
                try
                {
                    await _newsArticleRepository.BeginTransactionAsync();

                    string imagePath = string.Empty;

                    if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine("wwwroot", "uploads", "news");
                        Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await dto.ImageFile.CopyToAsync(fileStream);
                        }

                        imagePath = "/uploads/news/" + uniqueFileName;
                    }
                    else if (!string.IsNullOrEmpty(dto.ImageUrl))
                    {
                        imagePath = dto.ImageUrl.Trim();
                    }

                    var article = new NewsArticle
                    {
                        NewsArticleId = Guid.NewGuid().ToString(),
                        NewsTitle = dto.NewsTitle,
                        Headline = dto.Headline,
                        NewsContent = dto.NewsContent,
                        NewsSource = dto.NewsSource,
                        ImageTitle = imagePath,
                        CategoryId = (short)(dto.CategoryId),
                        CreatedById = user.AccountId,
                        NewsStatus = dto.NewsStatus,
                        CreatedDate = DateTime.Now
                    };
                var existingTags = await _tagRepository.GetExistingTagsByNamesAsync(dto.Tags);

                var newTags = dto.Tags
                    .Where(name => !existingTags.Any(e => e.TagName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    .Select(name => new Tag { TagId = Guid.NewGuid().ToString(), TagName = name })
                    .ToList();

                if (newTags.Any())
                {
                    await _tagRepository.AddTagsAsync(newTags);
                }

                _tagRepository.AttachTagsAsUnchanged(existingTags);

                article.Tags = existingTags.Concat(newTags).ToList();

                await _newsArticleRepository.AddAsync(article);
                await _newsArticleRepository.CommitTransactionAsync();


                return ApiResponseDto<string>.SuccessResponse("✅ Đăng bài thành công.");

                }
                catch (Exception ex)
                {
                    await _newsArticleRepository.RollbackTransactionAsync();
                    return ApiResponseDto<string>.FailResponse(StatusCodes.Status400BadRequest);
                }
            }
        public async Task<List<PostManageDto>> GetArticlesByStatusAsync(string status, string userId)
        {
            var articles = await _newsArticleRepository.GetByStatusAsync(status, userId);
            return articles.Select(x => new PostManageDto
            {
                Id = x.NewsArticleId,
                Title = x.NewsTitle,
                ImageUrl = x.ImageTitle,
                Status = x.NewsStatus,
                CreatedAt = x.CreatedDate?.ToString("dd/MM/yyyy") ?? "",
            }).ToList();
        }
    }
}
