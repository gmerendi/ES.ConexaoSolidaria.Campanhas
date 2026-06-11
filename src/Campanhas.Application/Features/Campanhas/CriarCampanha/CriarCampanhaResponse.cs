namespace Campanhas.Application.Features.Campanhas
{
    public record CriarCampanhaResponse(
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
