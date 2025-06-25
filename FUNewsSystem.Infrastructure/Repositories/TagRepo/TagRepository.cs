using FUNewsSystem.Domain.Models;
using FUNewsSystem.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Infrastructure.Repositories.TagRepo
{
    public class TagRepository : RepositoryBase<Tag>, ITagRepository
    {
        public TagRepository(FunewsSystemApiDbContext context) : base(context)
        {
        }

        public async Task AddTagsAsync(List<Tag> tags)
        {
            await _dbSet.AddRangeAsync(tags);
            await _context.SaveChangesAsync();
        }

        public void AttachTagsAsUnchanged(IEnumerable<Tag> tags)
        {
            foreach (var tag in tags)
            {
                if (_context.Entry(tag).State == EntityState.Detached)
                {
                    _dbSet.Attach(tag); 
                }
            }
        }


        public async Task<List<Tag>> GetExistingTagsByNamesAsync(List<string> tagNames)
        {
            return await _dbSet
                .Where(t => tagNames.Contains(t.TagName))
                .ToListAsync();
        }

    }
}
