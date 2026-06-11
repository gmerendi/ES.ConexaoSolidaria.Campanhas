using Campanhas.Domain.Shared.Interfaces;

namespace Campanhas.Domain.Entities.Campanhas
{
    public interface ICampanhaRepository : IRepository<Campanha>
    {
        Task<Campanha?> ObterPorTituloAsync(string email, CancellationToken ct = default);
        Task <List<Campanha>> ObterTodosAsync(int page, int pageLength, CancellationToken ct = default);
    }
}
