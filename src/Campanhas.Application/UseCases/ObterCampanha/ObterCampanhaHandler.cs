using Campanhas.Application.Common;
using Campanhas.Application.DTOs;
using Campanhas.Application.Extensions;
using Campanhas.Domain.Interfaces;

namespace Campanhas.Application.UseCases.ObterCampanha;

public sealed class ObterCampanhaHandler : IUseCaseHandler<ObterCampanhaQuery, CampanhaDto>
{
    private readonly ICampanhaRepository _repository;

    public ObterCampanhaHandler(ICampanhaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CampanhaDto>> HandleAsync(
        ObterCampanhaQuery query,
        CancellationToken cancellationToken = default)
    {
        var campanha = await _repository.GetByIdAsync(query.Id, cancellationToken);

        if (campanha is null)
            return Result<CampanhaDto>.NotFound($"Campanha '{query.Id}' não encontrada.");

        return Result<CampanhaDto>.Success(campanha.ToDto());
    }
}
