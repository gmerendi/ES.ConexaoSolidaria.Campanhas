using Campanhas.Application.Common;
using Campanhas.Application.DTOs;
using Campanhas.Application.Extensions;
using Campanhas.Domain.Interfaces;

namespace Campanhas.Application.UseCases.ObterTodasCampanhas;

public sealed class ObterTodasCampanhasHandler : IUseCaseHandler<ObterTodasCampanhasQuery, IReadOnlyList<CampanhaDto>>
{
    private readonly ICampanhaRepository _repository;

    public ObterTodasCampanhasHandler(ICampanhaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<CampanhaDto>>> HandleAsync(
        ObterTodasCampanhasQuery query,
        CancellationToken cancellationToken = default)
    {
        var campanhas = await _repository.GetAllAsync(cancellationToken);
        var dtos = campanhas.Select(c => c.ToDto()).ToList().AsReadOnly();
        return Result<IReadOnlyList<CampanhaDto>>.Success(dtos);
    }
}
