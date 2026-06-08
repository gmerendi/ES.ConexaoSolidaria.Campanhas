namespace Campanhas.Domain.Events;

public abstract class DomainEventBase : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcorridoEm { get; } = DateTime.UtcNow;
}
