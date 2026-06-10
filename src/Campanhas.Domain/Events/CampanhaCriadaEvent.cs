namespace Campanhas.Domain.Events;

public sealed class CampanhaCriadaEvent : DomainEventBase
{
    public Guid CampanhaId { get; }
    public string Titulo { get; }
    public decimal MetaFinanceira { get; }

    public CampanhaCriadaEvent(Guid campanhaId, string titulo, decimal metaFinanceira)
    {
        CampanhaId = campanhaId;
        Titulo = titulo;
        MetaFinanceira = metaFinanceira;
    }
}
