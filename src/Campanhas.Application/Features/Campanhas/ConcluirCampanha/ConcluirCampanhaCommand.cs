using System.ComponentModel.DataAnnotations;

namespace Campanhas.Application.Features.Campanhas;

public record ConcluirCampanhaCommand(
        [Required(ErrorMessage = "400_GUID_REQUIRED")]
        Guid Guid
    );
