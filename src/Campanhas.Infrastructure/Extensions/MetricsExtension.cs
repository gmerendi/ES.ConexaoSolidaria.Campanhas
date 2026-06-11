using Camapanhas.Infrastructure.Services.Metrics;
using Campanhas.Domain.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Campanhas.Infrastructure.Extensions;

public static class MetricsExtension
{
    /// <summary>
    /// Adiciona IMetricsService ao container. Chamado em Program.cs junto às outras extensions.
    /// </summary>
    public static IServiceCollection AddMetricsServices(this IServiceCollection services, ILogger logger)
    {
        services.AddSingleton<IMetricsService, MetricsService>();
        logger.LogInformation(" ***** Metrics service (Prometheus) inicializado.");
        return services;
    }
}
