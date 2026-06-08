using Campanhas.Application.Common;
using Campanhas.Application.DTOs;
using Campanhas.Application.Extensions;
using Campanhas.Domain.Interfaces;

namespace Campanhas.Application.UseCases.ObterCampanhasPorFiltro;

public sealed class ObterCampanhasPorFiltroHandler
    : IUseCaseHandler<ObterCampanhasPorFiltroQuery, IReadOnlyList<CampanhaDto>>
{
    private readonly ICampanhaRepository _repository;

    public ObterCampanhasPorFiltroHandler(ICampanhaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<CampanhaDto>>> HandleAsync(
        ObterCampanhasPorFiltroQuery query,
        CancellationToken cancellationToken = default)
    {
        var campanhas = await _repository.GetByFiltroAsync(
            query.Titulo,
            query.Status,
            query.DataInicioMin,
            query.DataInicioMax,
            cancellationToken);

        var dtos = campanhas.Select(c => c.ToDto()).ToList().AsReadOnly();
        return Result<IReadOnlyList<CampanhaDto>>.Success(dtos);
    }
}
