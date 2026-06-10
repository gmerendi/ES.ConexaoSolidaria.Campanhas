using Campanhas.Domain.Enums;

namespace Campanhas.Application.UseCases.ObterCampanhasPorFiltro;

public record ObterCampanhasPorFiltroQuery(
    string? Titulo = null,
    CampanhaStatus? Status = null,
    DateTime? DataInicioMin = null,
    DateTime? DataInicioMax = null
);
