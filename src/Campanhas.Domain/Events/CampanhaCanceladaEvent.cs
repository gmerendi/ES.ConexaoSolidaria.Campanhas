namespace Campanhas.Domain.Events;

public sealed class CampanhaCanceladaEvent : DomainEventBase
{
    public Guid CampanhaId { get; }
    public string Titulo { get; }

    public CampanhaCanceladaEvent(Guid campanhaId, string titulo)
    {
        CampanhaId = campanhaId;
        Titulo = titulo;
    }
}
