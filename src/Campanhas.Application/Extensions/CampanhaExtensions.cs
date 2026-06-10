using Campanhas.Application.DTOs;
using Campanhas.Domain.Entities;

namespace Campanhas.Application.Extensions;

public static class CampanhaExtensions
{
    public static CampanhaDto ToDto(this Campanha campanha) =>
        new(
            campanha.Id,
            campanha.Titulo.Valor,
            campanha.Descricao,
            campanha.MetaFinanceira.Valor,
            campanha.ValorArrecadado,
            campanha.DataInicio,
            campanha.DataFim,
            campanha.StatusCampanha.ToString(),
            campanha.Status.ToString(),
            campanha.DataCriacao,
            campanha.DataModificacao
        );
}
