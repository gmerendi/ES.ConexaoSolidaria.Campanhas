using System.ComponentModel.DataAnnotations;

namespace Campanhas.Application.Features.Campanhas;

public record CancelarCampanhaCommand(
    [Required(ErrorMessage = "400_GUID_REQUIRED")]
    Guid Guid
);
