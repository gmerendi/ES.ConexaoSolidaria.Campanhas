using Campanhas.Application.Common;
using Campanhas.Application.DTOs;
using Campanhas.Application.Extensions;
using Campanhas.Domain.Entities;
using Campanhas.Domain.Exceptions;
using Campanhas.Domain.Interfaces;
using Campanhas.Domain.ValueObjects;

namespace Campanhas.Application.UseCases.CriarCampanha;

public sealed class CriarCampanhaHandler : IUseCaseHandler<CriarCampanhaCommand, CampanhaDto>
{
    private readonly ICampanhaRepository _repository;

    public CriarCampanhaHandler(ICampanhaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CampanhaDto>> HandleAsync(
        CriarCampanhaCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var titulo = TituloCampanha.Criar(command.Titulo);
            var metaFinanceira = MetaFinanceira.Criar(command.MetaFinanceira);

            var campanha = Campanha.Criar(
                titulo,
                command.Descricao,
                metaFinanceira,
                command.DataInicio,
                command.DataFim);

            await _repository.AddAsync(campanha, cancellationToken);

            return Result<CampanhaDto>.Success(campanha.ToDto());
        }
        catch (DomainException ex)
        {
            return Result<CampanhaDto>.Failure(ex.Message);
        }
    }
}
