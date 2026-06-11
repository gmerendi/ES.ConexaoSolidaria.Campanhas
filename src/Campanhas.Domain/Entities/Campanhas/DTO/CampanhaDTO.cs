using Campanhas.Domain.Entities.Campanhas.Enums;
using Campanhas.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Campanhas.Domain.Entities.Campanhas
{
    public class CampanhaDTO
    {
        public Guid Guid { get; init; } 
        public string Titulo { get; init; } = String.Empty;
        public string Descricao { get; init; } = String.Empty;
        public decimal MetaFinanceira { get; init; } = 0;
        public decimal ValorArrecadado { get; init; } = 0;
        public string DataInicio { get; init; } = String.Empty;
        public string DataFim { get; init; } = String.Empty;
        public string StatusCampanha { get; init; }

        public CampanhaDTO() { }


        [SetsRequiredMembers]
        public CampanhaDTO(Guid guid, TituloCampanha titulo, string descricao, MetaFinanceira metaFinanceira,
            decimal valorArrecadado, DateTime dataInicio, DateTime dataFim, CampanhaStatus statusCampanha)
        {
            Guid = guid;
            Titulo = titulo.Valor;
            Descricao = descricao;
            MetaFinanceira = metaFinanceira.Valor;
            ValorArrecadado = valorArrecadado;
            DataInicio = dataInicio.ToString("yyyy-MMM-dd", CultureInfo.InvariantCulture);
            DataFim = dataFim.ToString("yyyy-MMM-dd", CultureInfo.InvariantCulture);
            StatusCampanha = statusCampanha.ToString();
        }

        public static CampanhaDTO FromEntity(Campanha campanha)
        {
            if (campanha == null) throw new ArgumentNullException(nameof(campanha));

            return new CampanhaDTO
            {
                Guid = campanha.Guid,
                Titulo = campanha.Titulo.Valor,
                Descricao = campanha.Descricao,
                MetaFinanceira = campanha.MetaFinanceira.Valor,
                ValorArrecadado = campanha.ValorArrecadado,
                DataInicio = campanha.DataInicio.ToString("yyyy-MMM-dd", CultureInfo.InvariantCulture),
                DataFim = campanha.DataFim.ToString("yyyy-MMM-dd", CultureInfo.InvariantCulture),
                StatusCampanha = campanha.StatusCampanha.ToString()
            };
        }
    }
}
