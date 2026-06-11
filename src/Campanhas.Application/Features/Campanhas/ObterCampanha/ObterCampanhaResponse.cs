namespace Campanhas.Application.Features.Campanhas
{
    public record ObterCampanhaResponse(
        Guid Guid,
        string Titulo,
        string Descricao,
        decimal MetaFinanceira,
        decimal ValorArrecadado,
        string DataInicio,
        string DataFim,
        string StatusCampanha
    );
}
