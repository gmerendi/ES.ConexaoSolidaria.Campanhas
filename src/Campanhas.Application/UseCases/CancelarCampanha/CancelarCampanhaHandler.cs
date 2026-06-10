using Campanhas.Application.Common;
using Campanhas.Application.DTOs;
using Campanhas.Application.Extensions;
using Campanhas.Domain.Exceptions;
using Campanhas.Domain.Interfaces;

namespace Campanhas.Application.UseCases.CancelarCampanha;

public sealed class CancelarCampanhaHandler : IUseCaseHandler<CancelarCampanhaCommand, CampanhaDto>
{
    private readonly ICampanhaRepository _repository;

    public CancelarCampanhaHandler(ICampanhaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CampanhaDto>> HandleAsync(
        CancelarCampanhaCommand command,
        CancellationToken cancellationToken = default)
    {
        var campanha = await _repository.GetByIdAsync(command.Id, cancellationToken);

        if (campanha is null)
            return Result<CampanhaDto>.NotFound($"Campanha '{command.Id}' não encontrada.");

        try
        {
            campanha.Cancelar();
            await _repository.UpdateAsync(campanha, cancellationToken);
            return Result<CampanhaDto>.Success(campanha.ToDto());
        }
        catch (DomainException ex)
        {
            return Result<CampanhaDto>.Failure(ex.Message);
        }
    }
}
