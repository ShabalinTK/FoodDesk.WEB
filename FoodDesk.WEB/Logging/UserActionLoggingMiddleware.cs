using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FoodDesk.WEB.Logging
{
    public class UserActionLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserActionLoggingMiddleware> _logger;

        public UserActionLoggingMiddleware(RequestDelegate next, ILogger<UserActionLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var user = context.User.Identity?.IsAuthenticated == true ? context.User.FindFirst(ClaimTypes.Email)?.Value ?? context.User.Identity.Name : "Anonymous";
            var path = context.Request.Path;
            var method = context.Request.Method;
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var time = DateTime.UtcNow;
            _logger.LogInformation("UserAction: {User} {Method} {Path} from {IP} at {Time}", user, method, path, ip, time);
            await _next(context);
        }
    }
} 