namespace Campanhas.Application.Features.Campanhas;

public record AlterarCampanhaCommand(
    Guid Guid,
    string Titulo,
    string Descricao,
    decimal MetaFinanceira,
    DateTime DataInicio,
    DateTime DataFim
);
