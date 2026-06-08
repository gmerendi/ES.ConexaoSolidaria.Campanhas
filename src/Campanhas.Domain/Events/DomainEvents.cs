namespace CS.Domain.Events
{
    public record DonationCreatedEvent(Guid guidUser, string nome, string email, string cpf, Guid guidCampanha, string nomeCampanha, string? correlationId);
    public record CampaignCreatedEvent(Guid guidCampanha, string nomeCampanha, string mensagemCampanha, string dataInicio, string dataTermino, decimal metaFinanceira, string? correlationId);
}
