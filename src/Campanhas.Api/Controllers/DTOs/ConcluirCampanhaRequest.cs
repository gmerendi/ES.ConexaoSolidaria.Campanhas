using System.ComponentModel.DataAnnotations;

namespace Campanhas.Api.Controllers.DTOs
{
    public record ConcluirCampanhaRequest(
        [Required(ErrorMessage = "400_GUID_REQUIRED")]
        Guid Guid
    );
}