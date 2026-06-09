using Campanhas.Domain.Enums;
using Campanhas.Domain.Events;

namespace Campanhas.Domain.Shared.Entity
{
    public class EntityBase
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public string CriadoPor { get; set; } = string.Empty;
        public string? ModificadoPor { get; set; }
        public DateTime? DataModificacao { get; set; }
        public EntityStatus Status { get; set; } = EntityStatus.ACTIVE;

        private readonly List<IDomainEvent> _domainEvents = [];
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
        public void ClearDomainEvents() => _domainEvents.Clear();
        protected void SetDataModificacao() => DataModificacao = DateTime.UtcNow;
    }
}
