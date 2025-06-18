using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using FUNewsSystem.Service.DTOs.ResponseDto;

namespace FUNewSystem.BE.Middlewares
{
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
        private readonly ILogger<CustomAuthorizationMiddlewareResultHandler> _logger;

        public CustomAuthorizationMiddlewareResultHandler(
            ILogger<CustomAuthorizationMiddlewareResultHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Succeeded)
            {
                await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
                return;
            }

            var request = context.Request;
            var response = context.Response;

            response.ContentType = "application/json";

            var statusCode = authorizeResult.Forbidden
                ? StatusCodes.Status403Forbidden 
                : StatusCodes.Status401Unauthorized;

            response.StatusCode = statusCode;

            string message = statusCode switch
            {
                StatusCodes.Status403Forbidden => "You don’t have permission",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                _ => "Error"
            };

            _logger.LogWarning($"Authorization failed with status {statusCode} for path {request.Path}");

            var error = new ErrorResponseDto
            {
                StatusCode = statusCode,
                Message = message,
                Path = request.Path.Value,
                Timestamp = DateTime.UtcNow
            };

            var result = JsonSerializer.Serialize(error, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteAsync(result);
        }
    }

}
