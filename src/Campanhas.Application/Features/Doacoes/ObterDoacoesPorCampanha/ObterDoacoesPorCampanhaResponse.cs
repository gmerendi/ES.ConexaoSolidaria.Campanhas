using Campanhas.Domain.Entities.Doacoes;

namespace Campanhas.Application.Features.Campanhas
{
    public record ObterDoacoesPorCampanhaResponse(
    List <DoacaoDTO> Doacoes
    );
}
