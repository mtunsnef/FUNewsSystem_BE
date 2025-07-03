using FUNewsSystem.Domain.Models;
using FUNewsSystem.Infrastructure.Repositories.SystemAccountRepo;
using FUNewsSystem.Service.DTOs.NewsArticleDto;
using FUNewsSystem.Service.Services.NewsArticleService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Jobs
{
    public class PostJob : IPostJob
    {
        private readonly INewsArticleService _postService;

        public PostJob(INewsArticleService postService)
        {
            _postService = postService;
        }

        public async Task ExecuteScheduledPostAsync(SystemAccount account, PostNewsArticleforUserDto dto)
        {
            await _postService.PostNewsArticleAsync(account, dto);
        }
    }
}
