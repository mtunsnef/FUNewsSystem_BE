using FUNewsSystem.Domain.Models;
using FUNewsSystem.Service.DTOs.NewsArticleDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Jobs
{
    public interface IPostJob
    {
        Task ExecuteScheduledPostAsync(SystemAccount account, PostNewsArticleforUserDto dto);
    }
}
