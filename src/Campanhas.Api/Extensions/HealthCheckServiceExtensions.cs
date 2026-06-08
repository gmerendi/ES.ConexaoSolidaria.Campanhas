using Campanhas.Api.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

namespace Campanhas.Api.Extensions;

public static class HealthCheckServiceExtensions
{
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgresql", tags: ["db", "infra"])
            .AddCheck<RedisHealthCheck>("redis", tags: ["cache", "infra"]);

        return services;
    }

    public static IEndpointRouteBuilder MapHealthCheckEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteJsonResponse
        });

        app.MapHealthChecks("/health/db", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("db"),
            ResponseWriter = WriteJsonResponse
        });

        app.MapHealthChecks("/health/cache", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("cache"),
            ResponseWriter = WriteJsonResponse
        });

        return app;
    }

    private static async Task WriteJsonResponse(HttpContext context, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            duracao = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                nome = e.Key,
                status = e.Value.Status.ToString(),
                descricao = e.Value.Description,
                duracao = e.Value.Duration.TotalMilliseconds
            })
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}
