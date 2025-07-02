using FUNewsSystem.Domain.Consts;
using FUNewsSystem.Domain.Models;
using FUNewsSystem.Service.DTOs.NewsArticleDto;
using FUNewsSystem.Service.DTOs.ResponseDto;
using FUNewsSystem.Service.Services.HttpContextService;
using FUNewsSystem.Service.Services.NewsArticleService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FUNewSystem.BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsArticleController : ControllerBase
    {
        private readonly INewsArticleService _newsArticleService;
        private readonly IHttpContextService _httpContextService;
        public NewsArticleController(INewsArticleService newsArticleService, IHttpContextService httpContextService)
        {
            _newsArticleService = newsArticleService;
            _httpContextService = httpContextService;
        }

        // GET: api/newsarticle
        [HttpGet]
        [EnableQuery]
        public async Task<ActionResult<IEnumerable<NewsArticle>>> GetAll()
        {
            var articles = await _newsArticleService.GetAllAsync();
            return Ok(articles);
        }

        [HttpGet("/odata/NewsArticle")]
        [EnableQuery]
        public ActionResult<IQueryable<NewsArticle>> GetAllOData()
        {
            return Ok(_newsArticleService.GetQueryable());
        }


        // GET: api/newsarticle/{id}
        [HttpGet("{id}")]
        [EnableQuery]
        public async Task<ActionResult<NewsArticle>> GetById(string id)
        {
            var article = await _newsArticleService.GetByIdAsync(id);
            return Ok(article);
        }

        // POST: api/newsarticle
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUpdateNewsArticleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _newsArticleService.CreateNewsArticle(dto);
            return Ok(response);
        }

        // PUT: api/newsarticle/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateUpdateNewsArticleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _newsArticleService.UpdateNewsArticle(id, dto);
            return Ok(response);
        }

        // DELETE: api/newsarticle/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var response = await _newsArticleService.DeleteNewsArticle(id);
            return Ok(response);
        }

        [HttpPost("post")]
        [Authorize(Roles = CustomRoles.Lecturer)]
        public async Task<IActionResult> PostNewsArticle([FromForm] PostNewsArticleforUserDto dto)
        {
            var systemAccount = await _httpContextService.GetSystemAccountAndThrow();
            var result = await _newsArticleService.PostNewsArticleAsync(systemAccount, dto);
            return Ok(result);
        }

        [HttpGet("check-access")]
        [Authorize(Roles = CustomRoles.Lecturer)]
        public IActionResult CheckAccess()
        {
            return Ok(ApiResponseDto<string>.SuccessResponse("Access granted"));
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = CustomRoles.Lecturer)]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var user = await _httpContextService.GetSystemAccountAndThrow();
            string statusChar = status.ToLower() switch
            {
                "draft" => "D",
                "pending" => "P",
                "active" => "A",
                "rejected" => "R",
                "cancelled" => "C",
                _ => throw new ArgumentException("Trạng thái không hợp lệ")
            };
            var result = await _newsArticleService.GetArticlesByStatusAsync(statusChar, user.AccountId);
            return Ok(result);
        }

        [HttpGet("update-status/{id}")]
        [Authorize(Roles = CustomRoles.Staff)]
        public async Task<IActionResult> UpdateStatus(string id)
        {
            var result = await _newsArticleService.UpdateStatusNewsArticle(id);
            return Ok(result);
        }
    }
}
