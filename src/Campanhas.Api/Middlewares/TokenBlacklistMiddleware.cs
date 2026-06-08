using Campanhas.Domain.Shared.Interfaces;
using System.Net;

namespace Campanhas.Api.Middlewares;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public TokenBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICacheService cacheService)
    {
        string authHeader = context.Request.Headers["Authorization"].ToString();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();

            bool isBlacklisted = await cacheService.IsBlacklistedAsync(token);

            if (isBlacklisted)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { message = "Token revogado. Por favor faça login novamente" });
                return;
            }
        }

        await _next(context);
    }
}


public static class TokenBlacklistMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenBlacklistMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TokenBlacklistMiddleware>();
    }
}
