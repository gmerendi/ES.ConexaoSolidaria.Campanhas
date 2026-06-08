namespace Campanhas.Application.UseCases.EditarCampanha;

public record EditarCampanhaCommand(
    Guid Id,
    string Titulo,
    string Descricao,
    decimal MetaFinanceira,
    DateTime DataInicio,
    DateTime DataFim
);
