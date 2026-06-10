namespace Campanhas.Domain.Events;

public sealed class CampanhaConcluidaEvent : DomainEventBase
{
    public Guid CampanhaId { get; }
    public string Titulo { get; }
    public decimal ValorArrecadado { get; }

    public CampanhaConcluidaEvent(Guid campanhaId, string titulo, decimal valorArrecadado)
    {
        CampanhaId = campanhaId;
        Titulo = titulo;
        ValorArrecadado = valorArrecadado;
    }
}
