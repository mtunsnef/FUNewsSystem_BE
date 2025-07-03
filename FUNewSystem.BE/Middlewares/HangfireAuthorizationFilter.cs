using Hangfire.Dashboard;

namespace FUNewSystem.BE.Middlewares
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // Cho phép tất cả (KHÔNG nên dùng cho production thật)
            return true;

            // Hoặc: chỉ cho user đã đăng nhập với quyền cụ thể
            // var httpContext = context.GetHttpContext();
            // return httpContext.User.Identity.IsAuthenticated;
        }
    }
}
