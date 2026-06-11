using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Campanhas.Infrastructure.Repositories;

public sealed class CampanhaRepository : EFRepository<Campanha>, ICampanhaRepository
{
    private readonly string _connectionString;

    public CampanhaRepository(ApplicationDbContext context, IConfiguration configuration) : base(context)
    {
        _connectionString = configuration.GetConnectionString("ConnectionString") ?? "";
    }




    public async Task<Campanha?> ObterPorTituloAsync(string titulo, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Titulo.Valor.ToLower() == titulo.ToLower(), ct);
    }


    public new async Task<List<Campanha>> ObterTodosAsync(int page = 1, int pageLength = 9999, CancellationToken cancellationToken = default)
    {

        return await _dbSet.Skip((page - 1) * pageLength)   // pula os itens das páginas anteriores
                     .Take(pageLength)                   // pega apenas o tamanho da página
                     .ToListAsync(cancellationToken);
    }





    /*
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
    */
}
