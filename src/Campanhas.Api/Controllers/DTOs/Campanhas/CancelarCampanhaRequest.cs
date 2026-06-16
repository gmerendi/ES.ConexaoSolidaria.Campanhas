using System.ComponentModel.DataAnnotations;

namespace Campanhas.Api.Controllers.DTOs
{
    public record CancelarCampanhaRequest(
        [Required(ErrorMessage = "400_GUID_REQUIRED")]
        Guid Guid
    );
}