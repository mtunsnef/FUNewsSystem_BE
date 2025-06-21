using FUNewsSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Infrastructure.Repositories.TagRepo
{
    public interface ITagRepository : IRepositoryBase<Tag>
    {
        Task AddTagsAsync(List<Tag> tags);
        Task<List<Tag>> GetExistingTagsByNamesAsync(List<string> tagNames);
        void AttachTagsAsUnchanged(IEnumerable<Tag> tags);
    }
}
