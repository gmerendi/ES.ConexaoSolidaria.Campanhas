using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Campanhas.Infrastructure.Extensions
{
    public static class DomainServicesExtension
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services, ILogger logger)
        {
           
            logger.LogInformation(" ***** Domain services inicializados.");

            return services;
        }

    }
}
