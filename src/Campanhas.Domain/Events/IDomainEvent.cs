namespace Campanhas.Domain.Events;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OcorridoEm { get; }
}
