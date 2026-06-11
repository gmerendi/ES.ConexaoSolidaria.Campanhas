using CS.Domain.Events;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Campanhas.Infrastructure.Services.Messaging
{
    public class MessageService : IMessageService
    {
        private readonly IPublishEndpoint _publish;
        private readonly string _userCreatedQueueUrl;
        private readonly string _userRemovedQueueUrl;
        private readonly string _applicationType;
        private readonly IBaseLogger<MessageService> _logger;
        private readonly ICorrelationIdGenerator _correlationIdGenerator;


        public MessageService(IPublishEndpoint publish, IConfiguration configuration
            , IBaseLogger<MessageService> logger, ICorrelationIdGenerator correlationIdGenerator)
        {
            _publish = publish;
            _userCreatedQueueUrl = Environment.GetEnvironmentVariable("USER_CREATED_QUEUE")
                                   ?? configuration["USER_CREATED_QUEUE"]
                                   ?? "user-queue-failed";
            _userRemovedQueueUrl = Environment.GetEnvironmentVariable("USER_REMOVED_QUEUE")
                                   ?? configuration["USER_REMOVED_QUEUE"]
                                   ?? "user-queue-failed";
            _applicationType = Environment.GetEnvironmentVariable("Application__Type")
                                   ?? configuration["Application__Type"]
                                   ?? "application_type_failed";
            _correlationIdGenerator = correlationIdGenerator;
            _logger = logger;
        }



        public async Task SendDonationCreatedEventMessage(Guid guidUser, string nome, string email, Guid guidCampanha, string tituloCampanha, CancellationToken ct)
        {

            try
            {
                var eventMessage = new DonationCreatedEvent(guidUser, nome, email, guidCampanha, tituloCampanha, _correlationIdGenerator.Get());
                await _publish.Publish(eventMessage, ct);
                _logger.LogInformation("Evento DonationCreatedEvent publicado para o Broker. Email: " + email, BaseLogType.EVENT, eventMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao publicar evento DonationCreatedEvent para o Broker : " + email, BaseLogType.EVENT, ex);
                throw;
            }

        }


        public async Task SendCampaignCreatedEventMessage(Guid guidCampanha, string tituloCampanha, string descricaoCampanha, DateTime dataInicio, DateTime dataTermino, decimal metaFinanceira, CancellationToken ct)
        {

            try
            {
                var eventMessage = new CampaignCreatedEvent(guidCampanha, tituloCampanha, descricaoCampanha, dataInicio, dataTermino, metaFinanceira, _correlationIdGenerator.Get());
                await _publish.Publish(eventMessage, ct);
                _logger.LogInformation("Evento CampaignCreatedEvent publicado para o Broker. Nome: " + descricaoCampanha, BaseLogType.EVENT, eventMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao publicar evento CampaignCreatedEvent para o Broker : " + descricaoCampanha, BaseLogType.EVENT, ex);
                throw;
            }

        }
    }
}
