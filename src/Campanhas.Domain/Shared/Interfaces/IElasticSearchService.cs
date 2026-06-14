using Campanhas.Domain.Entities.Campanhas;

namespace Campanhas.Domain.Shared.Interfaces
{
    public interface IElasticSearchService
    {
        Task<IEnumerable<CampanhaSemArrecadacaoDTO>> SearchAsync(string term);
        Task IndexAsync(CampanhaDTO term);
        Task<bool> IndexExistsAsync(string index);
        Task<long> CountAsync();
        Task<bool?> ClearIndiceAsync(string index);
        Task CreateIndexIfNotExistsAsync(string index);
        Task UpdateAsync(CampanhaDTO term);
    }
}
