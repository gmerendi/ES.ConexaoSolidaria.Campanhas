using System.ComponentModel.DataAnnotations;

namespace Campanhas.Api.Controllers.DTOs
{
    public record ObterDoacoesPorCampanhaRequest(
        [Required(ErrorMessage = "400_GUID_REQUIRED")]
        Guid GuidCampanha
    );
}