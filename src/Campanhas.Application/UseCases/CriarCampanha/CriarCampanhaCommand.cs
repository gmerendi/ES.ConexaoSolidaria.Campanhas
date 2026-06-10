namespace Campanhas.Application.UseCases.CriarCampanha;

public record CriarCampanhaCommand(
    string Titulo,
    string Descricao,
    decimal MetaFinanceira,
    DateTime DataInicio,
    DateTime DataFim
);
