using Campanhas.Domain.Shared.Entity;

namespace Campanhas.Domain.Interfaces;

public interface IRepository<T> where T : EntityBase
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
}
