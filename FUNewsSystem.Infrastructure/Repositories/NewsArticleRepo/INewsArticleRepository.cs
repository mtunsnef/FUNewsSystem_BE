using FUNewsSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Infrastructure.Repositories.NewsArticleRepo
{
    public interface INewsArticleRepository : IRepositoryBase<NewsArticle>
    {
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        IQueryable<NewsArticle> GetQueryable();
        Task<List<NewsArticle>> GetByStatusAsync(string status, string userId);
    }
}
