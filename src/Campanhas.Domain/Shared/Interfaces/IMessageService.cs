namespace Campanhas.Domain.Shared.Interfaces
{
    public interface IMessageService
    {
        Task SendDonationCreatedEventMessage(Guid guidUser, string nome, string email, string cpf, Guid guidCampanha, string nomeCampanha, CancellationToken ct);
        Task SendCampaignCreatedEventMessage(Guid guidCampanha, string nomeCampanha, string mensagemCampanha, string dataInicio, string dataTermino, decimal metaFinanceira, CancellationToken ct);
    }
}
