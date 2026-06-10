using Campanhas.Application.Common;
using Campanhas.Application.DTOs;
using Campanhas.Application.UseCases.CancelarCampanha;
using Campanhas.Application.UseCases.ConcluirCampanha;
using Campanhas.Application.UseCases.CriarCampanha;
using Campanhas.Application.UseCases.EditarCampanha;
using Campanhas.Application.UseCases.ObterCampanha;
using Campanhas.Application.UseCases.ObterCampanhasPorFiltro;
using Campanhas.Application.UseCases.ObterTodasCampanhas;

namespace Campanhas.Api.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddUseCaseServices(this IServiceCollection services)
    {
        services.AddScoped<IUseCaseHandler<CriarCampanhaCommand, CampanhaDto>, CriarCampanhaHandler>();
        services.AddScoped<IUseCaseHandler<EditarCampanhaCommand, CampanhaDto>, EditarCampanhaHandler>();
        services.AddScoped<IUseCaseHandler<CancelarCampanhaCommand, CampanhaDto>, CancelarCampanhaHandler>();
        services.AddScoped<IUseCaseHandler<ConcluirCampanhaCommand, CampanhaDto>, ConcluirCampanhaHandler>();
        services.AddScoped<IUseCaseHandler<ObterCampanhaQuery, CampanhaDto>, ObterCampanhaHandler>();
        services.AddScoped<IUseCaseHandler<ObterTodasCampanhasQuery, IReadOnlyList<CampanhaDto>>, ObterTodasCampanhasHandler>();
        services.AddScoped<IUseCaseHandler<ObterCampanhasPorFiltroQuery, IReadOnlyList<CampanhaDto>>, ObterCampanhasPorFiltroHandler>();

        return services;
    }
}
