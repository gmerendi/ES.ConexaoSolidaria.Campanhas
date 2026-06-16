using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Entities.Doacoes;
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

            services.AddScoped<IDoacaoRepository, DoacaoRepository>();
            logger.LogInformation(" ***** DoacaoRepository service inicializado.");

            return services;
        }
    }
}