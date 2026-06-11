using System.ComponentModel.DataAnnotations;

namespace Campanhas.Api.Controllers.DTOs
{
    public record AlterarCampanhaRequest(
        [Required(ErrorMessage = "400_GUID_REQUIRED")]
        Guid Guid,

        [Required(ErrorMessage = "400_TITLE_REQUIRED")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "400_TITLE_LENGTH_INVALID")]
        string Titulo,

        [Required(ErrorMessage = "400_TITLE_REQUIRED")]
        [StringLength(2000, MinimumLength = 0, ErrorMessage = "400_DESCRIPTION_LENGTH_INVALID")]
        string Descricao,

        [Required(ErrorMessage = "400_TARGET_REQUIRED")]
        [Range(0.01, 9999999999999999.99, ErrorMessage = "400_TARGET_RANGE_INVALID")]
        decimal MetaFinanceira,

        [Required(ErrorMessage = "400_STARTDATE_REQUIRED")]
        DateTime DataInicio,

        [Required(ErrorMessage = "400_ENDDATE_REQUIRED")]
        DateTime DataFim
    );
}