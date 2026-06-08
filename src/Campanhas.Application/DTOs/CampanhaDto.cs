namespace Campanhas.Application.DTOs;

public record CampanhaDto(
    Guid Id,
    string Titulo,
    string Descricao,
    decimal MetaFinanceira,
    decimal ValorArrecadado,
    DateTime DataInicio,
    DateTime DataFim,
    string StatusCampanha,
    string StatusEntidade,
    DateTime DataCriacao,
    DateTime DataModificacao
);
