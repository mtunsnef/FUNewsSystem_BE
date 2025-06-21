using FUNewsSystem.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

namespace FUNewsSystem.Infrastructure.Repositories.NewsArticleRepo
{
    public class NewsArticleRepository : RepositoryBase<NewsArticle>, INewsArticleRepository
    {
        private IDbContextTransaction? _transaction;

        public NewsArticleRepository(FunewsSystemApiDbContext context) : base(context)
        {
        }
        public IQueryable<NewsArticle> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
                await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }

        public async Task<List<NewsArticle>> GetByStatusAsync(string status, string userId)
        {
            return await _context.NewsArticles
                .Where(x => x.NewsStatus == status && x.CreatedById == userId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }
    }
}
