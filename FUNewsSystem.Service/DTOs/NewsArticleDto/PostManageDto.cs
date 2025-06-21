using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.NewsArticleDto
{
    public class PostManageDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }
    }
}
