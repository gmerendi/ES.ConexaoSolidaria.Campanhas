using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Infrastructure.Services.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Campanhas.Infrastructure.Extensions
{
    public static class LoggingExtensions
    {
        public static IServiceCollection AddCustomLogging(this IServiceCollection services, ILogger logger)
        {
            services.AddTransient<ICorrelationIdGenerator, CorrelationIdGenerator>();
            logger.LogInformation(" ***** CorrelationIdGenerator service inicializado.");
            
            services.AddTransient(typeof(IBaseLogger<>), typeof(BaseLogger<>));
            logger.LogInformation(" ***** BaseLogger service inicializado.");

            return services;
        }
    }
}
