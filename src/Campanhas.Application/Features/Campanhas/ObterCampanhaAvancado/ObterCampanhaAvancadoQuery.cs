using System.ComponentModel.DataAnnotations;

namespace Campanhas.Application.Features.Campanhas;

public record ObterCampanhaAvancadoQuery(
        [Required(ErrorMessage = "400_TERM_REQUIRED")]
        string Termo
    );
