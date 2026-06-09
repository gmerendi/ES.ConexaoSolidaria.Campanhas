using Campanhas.Domain.Entities;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Interfaces;
using Campanhas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Campanhas.Infrastructure.Repositories;

public sealed class CampanhaRepository : ICampanhaRepository
{
    private readonly CampanhaDbContext _context;

    public CampanhaRepository(CampanhaDbContext context)
    {
        _context = context;
    }

    public async Task<Campanha?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Campanhas.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IEnumerable<Campanha>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Campanhas.ToListAsync(ct);

    public async Task AddAsync(Campanha entity, CancellationToken ct = default)
    {
        await _context.Campanhas.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Campanha entity, CancellationToken ct = default)
    {
        _context.Campanhas.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Campanha>> GetByStatusAsync(
        CampanhaStatus status,
        CancellationToken ct = default) =>
        await _context.Campanhas
            .Where(c => c.StatusCampanha == status)
            .ToListAsync(ct);

    public async Task<IEnumerable<Campanha>> GetByFiltroAsync(
        string? titulo,
        CampanhaStatus? status,
        DateTime? dataInicioMin,
        DateTime? dataInicioMax,
        CancellationToken ct = default)
    {
        var query = _context.Campanhas.AsQueryable();

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
