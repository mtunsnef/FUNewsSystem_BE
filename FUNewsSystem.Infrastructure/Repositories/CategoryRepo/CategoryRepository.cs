using FUNewsSystem.Domain.Models;
using FUNewsSystem.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Infrastructure.Repositories.CategoryRepo
{
    public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
    {
        public CategoryRepository(FunewsSystemApiDbContext context) : base(context)
        { }
        //public override IQueryable<Category> GetAll()
        //{
        //    return _dbSet.Include(n => n.NewsArticles).AsQueryable();
        //}
    }
}
