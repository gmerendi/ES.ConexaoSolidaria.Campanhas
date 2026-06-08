using Campanhas.Domain.Entities;
using Campanhas.Domain.Enums;

namespace Campanhas.Domain.Interfaces;

public interface ICampanhaRepository : IRepository<Campanha>
{
    Task<IEnumerable<Campanha>> GetByStatusAsync(CampanhaStatus status, CancellationToken ct = default);
    Task<IEnumerable<Campanha>> GetByFiltroAsync(
        string? titulo,
        CampanhaStatus? status,
        DateTime? dataInicioMin,
        DateTime? dataInicioMax,
        CancellationToken ct = default);
}
