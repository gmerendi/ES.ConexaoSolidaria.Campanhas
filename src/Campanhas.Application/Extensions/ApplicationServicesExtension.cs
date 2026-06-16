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
            services.AddScoped<IUseCaseHandler<ObterCampanhaAvancadoQuery, Result<ObterCampanhaAvancadoResponse>>, ObterCampanhaAvancadoQueryHandler>();
            services.AddScoped<IUseCaseHandler<CriarDoacaoCommand, Result<CriarDoacaoResponse>>, CriarDoacaoCommandHandler>();
            services.AddScoped<IUseCaseHandler<ObterDoacoesPorCampanhaQuery, Result<ObterDoacoesPorCampanhaResponse>>, ObterDoacoesPorCampanhaQueryHandler>();
            services.AddScoped<IUseCaseHandler<ObterDoacoesPorUsuarioQuery, Result<ObterDoacoesPorUsuarioResponse>>, ObterDoacoesPorUsuarioQueryHandler>();

            logger.LogInformation(" ***** UseCase services inicializados.");

            return services;
        }

    }
}
