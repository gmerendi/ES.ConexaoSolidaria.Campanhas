namespace CS.Domain.Events
{
    public record DonationCreatedEvent(Guid guidUser, string nome, string email, Guid guidCampanha, string tituloCampanha, string cpf, decimal valor, string? correlationId);
    public record CampaignCreatedEvent(Guid guidCampanha, string nomeCampanha, string mensagemCampanha, DateTime dataInicio, DateTime dataTermino, decimal metaFinanceira, string? correlationId);
}
