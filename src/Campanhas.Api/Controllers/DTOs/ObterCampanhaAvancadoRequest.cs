using System.ComponentModel.DataAnnotations;

namespace Campanhas.Api.Controllers.DTOs
{
    public record ObterCampanhaAvancadoRequest(
        [Required(ErrorMessage = "400_TERM_REQUIRED")]
        string Termo
    );
}