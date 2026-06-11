using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Campanhas.Infrastructure.Extensions
{
    public static class RepositoryExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services, ILogger logger)
        {
            services.AddScoped<ICampanhaRepository, CampanhaRepository>();
            logger.LogInformation(" ***** CampanhaRepository service inicializado.");
            return services;
        }
    }
}