using System.ComponentModel.DataAnnotations;

namespace Campanhas.Api.Controllers.DTOs
{
    public record ObterDoacoesPorUsuarioRequest(
        [Required(ErrorMessage = "400_GUID_REQUIRED")]
        Guid GuidUsuario
    );
}