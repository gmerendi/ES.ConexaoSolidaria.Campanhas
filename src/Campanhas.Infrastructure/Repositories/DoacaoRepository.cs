using Campanhas.Domain.Entities.Doacoes;
using Campanhas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Campanhas.Infrastructure.Repositories;

public sealed class DoacaoRepository : EFRepository<Doacao>, IDoacaoRepository
{
    private readonly string _connectionString;

    public DoacaoRepository(ApplicationDbContext context, IConfiguration configuration) : base(context)
    {
        _connectionString = configuration.GetConnectionString("ConnectionString") ?? "";
    }



    public async Task<Doacao?> ObterPorCorrelationIdAsync(string correlationId, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.CorrelationId.ToLower() == correlationId.ToLower(), ct);
    }


    public async Task<List<DoacaoDTO>> ObterPorCampanhaAsync(Guid guidCampanha, CancellationToken ct = default)
    {
        return await _dbSet
         .Where(u => u.GuidCampanha == guidCampanha)
         .Select(u => new DoacaoDTO(
             u.GuidUsuario,
             u.NomeUsuario,
             u.EmailUsuario.Endereco,
             u.CpfUsuario.Numero,
             u.GuidCampanha,
             u.TituloCampanha.Valor,   
             u.ValorDoacao,
             u.DataCriacao.ToString("dd/MM/yyyy HH:mm:ss") 
         ))
         .ToListAsync(ct);
    }


    public async Task<List<DoacaoShortDTO>> ObterPorUsuarioAsync(Guid guidUsuario, CancellationToken ct = default)
    {
        return await _dbSet
         .Where(u => u.GuidUsuario == guidUsuario)
         .Select(u => new DoacaoShortDTO(
             u.GuidCampanha,
             u.TituloCampanha.Valor,
             u.ValorDoacao,
             u.DataCriacao.ToString("dd/MM/yyyy HH:mm:ss")
         ))
         .ToListAsync(ct);
    }
}
