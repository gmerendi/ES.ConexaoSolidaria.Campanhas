using System.ComponentModel.DataAnnotations;

namespace Campanhas.Api.Controllers.DTOs
{
    public record RemoverCampanhaRequest(
        [Required(ErrorMessage = "400_GUID_REQUIRED")]
        Guid Guid
    );
}