using Campanhas.Domain.Entities.Doacoes;

namespace Campanhas.Application.Features.Campanhas
{
    public record ObterDoacoesPorUsuarioResponse(
    List <DoacaoShortDTO> Doacoes
    );
}
