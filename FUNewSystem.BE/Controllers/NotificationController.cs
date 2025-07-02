using FUNewsSystem.Domain.Consts;
using FUNewsSystem.Service.Services.HttpContextService;
using FUNewsSystem.Service.Services.NotificationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace FUNewSystem.BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IHttpContextService _httpContextService;

        public NotificationController(INotificationService notificationService, IHttpContextService httpContextService)
        {
            _notificationService = notificationService;
            _httpContextService = httpContextService;
        }

        [HttpGet]
        [EnableQuery]
        [Authorize(Roles = CustomRoles.Staff)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _notificationService.GetAllNotificationAsync();
            return Ok(result);
        }

        [HttpGet("get-notification")]
        [EnableQuery]
        [Authorize(Roles = CustomRoles.Lecturer)]
        public async Task<IActionResult> GetAllByUserId()
        {
            var account = await _httpContextService.GetSystemAccountAndThrow();
            var result = await _notificationService.GetNotificationsByUserIdAsync(account.AccountId);
            return Ok(result);
        }
    }
}
