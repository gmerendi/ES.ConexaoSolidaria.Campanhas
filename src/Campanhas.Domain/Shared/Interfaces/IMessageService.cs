namespace Campanhas.Domain.Shared.Interfaces
{
    public interface IMessageService
    {
        Task SendDonationCreatedEventMessage(Guid guidUser, string nome, string email, string cpf, Guid guidCampanha, string nomeCampanha, CancellationToken ct);
        Task SendCampaignCreatedEventMessage(Guid guidCampanha, string tituloCampanha, string descricaoCampanha, DateTime dataInicio, DateTime dataTermino, decimal metaFinanceira, CancellationToken ct);
    }
}
