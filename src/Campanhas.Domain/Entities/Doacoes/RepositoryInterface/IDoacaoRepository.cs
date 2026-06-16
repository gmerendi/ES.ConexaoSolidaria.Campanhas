using Campanhas.Domain.Shared.Interfaces;

namespace Campanhas.Domain.Entities.Doacoes
{
    public interface IDoacaoRepository : IRepository<Doacao>
    {
        Task<Doacao?> ObterPorCorrelationIdAsync(string correlationId, CancellationToken ct = default);
        Task<List<DoacaoDTO>> ObterPorCampanhaAsync(Guid guidCampanha, CancellationToken ct = default);
        Task<List<DoacaoShortDTO>> ObterPorUsuarioAsync(Guid guidUsuario, CancellationToken ct = default);
    }
}
