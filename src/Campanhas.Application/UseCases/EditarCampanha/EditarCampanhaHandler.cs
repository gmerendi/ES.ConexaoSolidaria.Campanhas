using Campanhas.Application.Common;
using Campanhas.Application.DTOs;
using Campanhas.Application.Extensions;
using Campanhas.Domain.Exceptions;
using Campanhas.Domain.Interfaces;
using Campanhas.Domain.ValueObjects;

namespace Campanhas.Application.UseCases.EditarCampanha;

public sealed class EditarCampanhaHandler : IUseCaseHandler<EditarCampanhaCommand, CampanhaDto>
{
    private readonly ICampanhaRepository _repository;

    public EditarCampanhaHandler(ICampanhaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CampanhaDto>> HandleAsync(
        EditarCampanhaCommand command,
        CancellationToken cancellationToken = default)
    {
        var campanha = await _repository.GetByIdAsync(command.Id, cancellationToken);

        if (campanha is null)
            return Result<CampanhaDto>.NotFound($"Campanha '{command.Id}' não encontrada.");

        try
        {
            var titulo = TituloCampanha.Criar(command.Titulo);
            var metaFinanceira = MetaFinanceira.Criar(command.MetaFinanceira);

            campanha.Editar(titulo, command.Descricao, metaFinanceira, command.DataInicio, command.DataFim);

            await _repository.UpdateAsync(campanha, cancellationToken);

            return Result<CampanhaDto>.Success(campanha.ToDto());
        }
        catch (DomainException ex)
        {
            return Result<CampanhaDto>.Failure(ex.Message);
        }
    }
}
