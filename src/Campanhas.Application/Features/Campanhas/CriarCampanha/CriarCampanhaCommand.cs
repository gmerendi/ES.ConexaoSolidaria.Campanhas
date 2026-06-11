namespace Campanhas.Application.Features.Campanhas;

public record CriarCampanhaCommand(
    string Titulo,
    string Descricao,
    decimal MetaFinanceira,
    DateTime DataInicio,
    DateTime DataFim
);
