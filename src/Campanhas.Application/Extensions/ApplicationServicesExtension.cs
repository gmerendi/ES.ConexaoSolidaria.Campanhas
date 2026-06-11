using Campanhas.Application.Features.Campanhas;
using Campanhas.Application.Shared;
using Campanhas.Domain.Shared.Primitives;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Camapanhas.Application.Extensions
{
    public static class ApplicationServicesExtension
    {
        public static IServiceCollection AddUseCaseServices(this IServiceCollection services, ILogger logger)
        {
            // ApplicationServices
            

            services.AddScoped<IUseCaseHandler<CriarCampanhaCommand, Result<CriarCampanhaResponse>>, CriarCampanhaCommandHandler>();
            services.AddScoped<IUseCaseHandler<AlterarCampanhaCommand, Result<AlterarCampanhaResponse>>, AlterarCampanhaCommandHandler>();
            services.AddScoped<IUseCaseHandler<ObterCampanhaQuery, Result<ObterCampanhaResponse>>, ObterCampanhaQueryHandler>();
            services.AddScoped<IUseCaseHandler<ObterTodasCampanhasQuery, Result<ObterTodasCampanhasResponse>>, ObterTodasCampanhasQueryHandler>();
            services.AddScoped<IUseCaseHandler<ConcluirCampanhaCommand, Result<bool>>, ConcluirCampanhaCommandHandler>();
            services.AddScoped<IUseCaseHandler<CancelarCampanhaCommand, Result<bool>>, CancelarCampanhaCommandHandler>();


            logger.LogInformation(" ***** UseCase services inicializados.");

            return services;
        }

    }
}
