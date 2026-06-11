using Campanhas.Domain.Entities.Campanhas;

namespace Campanhas.Application.Features.Campanhas
{
    public record ObterTodasCampanhasResponse(
    List <CampanhaDTO> Campanhas
    );
}
