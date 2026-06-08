using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Campanhas.Infrastructure.Extensions
{
    public static class RepositoryExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services, ILogger logger)
        {
            
            //services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            logger.LogInformation(" ***** UsuarioRepository service inicializado.");

            return services;
        }
    }
}
