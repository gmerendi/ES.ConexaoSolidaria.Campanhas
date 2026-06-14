using Campanhas.Domain.Entities.Campanhas;

namespace Campanhas.Application.Features.Campanhas
{
    public record ObterCampanhaAvancadoResponse(
    IEnumerable<CampanhaSemArrecadacaoDTO> Campanhas
    );
}
