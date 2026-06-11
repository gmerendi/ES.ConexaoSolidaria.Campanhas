using Campanhas.Domain.Shared.Resources;
using System.ComponentModel.DataAnnotations;

namespace Campanhas.Application.Features.Campanhas;

public record CriarDoacaoCommand(
        [Required(ErrorMessage = "400_GUID_REQUIRED")]
        Guid Guid,

        [Required(ErrorMessage = "400_VALUE_REQUIRED")]
        [Range(0.01, double.MaxValue, ErrorMessage = "422_VALUE_MUST_BE_GREATER_THAN_ZERO")]
       decimal Valor
);
