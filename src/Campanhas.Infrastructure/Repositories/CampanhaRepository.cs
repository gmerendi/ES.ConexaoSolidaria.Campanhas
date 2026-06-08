using Campanhas.Domain.Entities;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Interfaces;
using Campanhas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Campanhas.Infrastructure.Repositories;

public sealed class CampanhaRepository : EFRepository<Campanha>, ICampanhaRepository
{
    public CampanhaRepository(CampanhaDbContext context) : base(context) { }

    public async Task<IEnumerable<Campanha>> GetByStatusAsync(
        CampanhaStatus status,
        CancellationToken ct = default) =>
        await DbSet
            .Where(c => c.StatusCampanha == status)
            .ToListAsync(ct);

    public async Task<IEnumerable<Campanha>> GetByFiltroAsync(
        string? titulo,
        CampanhaStatus? status,
        DateTime? dataInicioMin,
        DateTime? dataInicioMax,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(titulo))
            query = query.Where(c => c.Titulo.Valor.Contains(titulo));

        if (status.HasValue)
            query = query.Where(c => c.StatusCampanha == status.Value);

        if (dataInicioMin.HasValue)
            query = query.Where(c => c.DataInicio >= dataInicioMin.Value);

        if (dataInicioMax.HasValue)
            query = query.Where(c => c.DataInicio <= dataInicioMax.Value);

        return await query.ToListAsync(ct);
    }
}
