using Campanhas.Domain.Entities.Campanhas.Enums;
using Campanhas.Domain.Entities.ValueObjects;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Campanhas.Domain.Entities.Campanhas
{
    public class CampanhaSemArrecadacaoDTO
    {
        public Guid Guid { get; init; } 
        public string Titulo { get; init; } = String.Empty;
        public string Descricao { get; init; } = String.Empty;
        public decimal MetaFinanceira { get; init; } = 0;
        public string DataInicio { get; init; } = String.Empty;
        public string DataFim { get; init; } = String.Empty;
        public string StatusCampanha { get; init; }

        public CampanhaSemArrecadacaoDTO() { }


        [SetsRequiredMembers]
        public CampanhaSemArrecadacaoDTO(Guid guid, TituloCampanha titulo, string descricao, decimal metaFinanceira,
            DateTime dataInicio, DateTime dataFim, CampanhaStatus statusCampanha)
        {
            Guid = guid;
            Titulo = titulo.Valor;
            MetaFinanceira = metaFinanceira;
            Descricao = descricao;
            DataInicio = dataInicio.ToString("yyyy-MMM-dd", CultureInfo.InvariantCulture);
            DataFim = dataFim.ToString("yyyy-MMM-dd", CultureInfo.InvariantCulture);
            StatusCampanha = statusCampanha.ToString();
        }

        public static CampanhaSemArrecadacaoDTO FromCampanhaDTO(CampanhaDTO campanha)
        {
            if (campanha == null) throw new ArgumentNullException(nameof(campanha));

            return new CampanhaSemArrecadacaoDTO
            {
                Guid = campanha.Guid,
                Titulo = campanha.Titulo,
                Descricao = campanha.Descricao,
                MetaFinanceira = campanha.MetaFinanceira,
                DataInicio = campanha.DataInicio,
                DataFim = campanha.DataFim,
                StatusCampanha = campanha.StatusCampanha.ToString()
            };
        }
    }
}
