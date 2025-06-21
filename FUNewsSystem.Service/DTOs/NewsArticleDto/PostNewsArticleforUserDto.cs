using FUNewsSystem.Domain.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.DTOs.NewsArticleDto
{
    public class PostNewsArticleforUserDto
    {
        public string? NewsArticleId { get; set; }
        public string NewsTitle { get; set; }
        public string Headline { get; set; }
        public string NewsContent { get; set; }
        public string NewsSource { get; set; }
        public int CategoryId { get; set; }
        public IFormFile? ImageFile { get; set; } // dùng cho file upload
        public string? ImageUrl { get; set; } // nếu chọn từ URL
        public List<string> Tags { get; set; } = new List<string>();
        public string NewsStatus { get; set; } 
    }
}
