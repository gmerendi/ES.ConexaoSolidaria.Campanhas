using Campanhas.Domain.Enums;
using Campanhas.Domain.Events;

namespace Campanhas.Domain;

public abstract class EntityBase
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime DataCriacao { get; protected set; } = DateTime.UtcNow;
    public DateTime DataModificacao { get; protected set; } = DateTime.UtcNow;
    public EntityStatus Status { get; protected set; } = EntityStatus.Ativo;

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
    protected void SetDataModificacao() => DataModificacao = DateTime.UtcNow;
}
