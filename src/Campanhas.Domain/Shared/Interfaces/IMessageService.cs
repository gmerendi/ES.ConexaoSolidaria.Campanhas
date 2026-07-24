namespace Campanhas.Domain.Shared.Interfaces
{
    public interface IMessageService
    {
        Task SendDonationCreatedEventMessage(Guid guidUser, string nome, string email, Guid guidCampanha, string nomeCampanha, string cpf, decimal valor, string status, CancellationToken ct);
    }
}
