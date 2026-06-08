namespace Campanhas.Api.Models;

public record CriarCampanhaRequest(
    string Titulo,
    string Descricao,
    decimal MetaFinanceira,
    DateTime DataInicio,
    DateTime DataFim
);

public record EditarCampanhaRequest(
    string Titulo,
    string Descricao,
    decimal MetaFinanceira,
    DateTime DataInicio,
    DateTime DataFim
);
