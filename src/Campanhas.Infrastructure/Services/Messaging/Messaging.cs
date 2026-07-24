using Amazon.SQS;
using Amazon.SQS.Model;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.ValueObjects;
using CS.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Campanhas.Infrastructure.Services.Messaging
{
    public class MessageService : IMessageService
    {
        private readonly IPublishEndpoint _publish;
        private readonly string _donationCreatedQueueUrl;
        private readonly string _userRemovedQueueUrl;
        private readonly string _applicationType;
        private readonly IBaseLogger<MessageService> _logger;
        private readonly ICorrelationIdGenerator _correlationIdGenerator;
        private readonly IAmazonSQS _sqsClient;


        public MessageService(IPublishEndpoint publish, IConfiguration configuration,
             IBaseLogger<MessageService> logger, ICorrelationIdGenerator correlationIdGenerator,
             IAmazonSQS sqsClient)
        {
            _publish = publish;
            _donationCreatedQueueUrl = Environment.GetEnvironmentVariable("DONATION_CREATED_QUEUE")
                                   ?? configuration["DONATION_CREATED_QUEUE"]
                                   ?? "user-queue-failed";
            _applicationType = Environment.GetEnvironmentVariable("Application__Type")
                                   ?? configuration["Application__Type"]
                                   ?? "application_type_failed";
            _correlationIdGenerator = correlationIdGenerator;
            _logger = logger;
            _sqsClient = sqsClient;

        }



        public async Task SendDonationCreatedEventMessage(Guid guidUser, string nome, string email, Guid guidCampanha, string tituloCampanha, string cpf, decimal valor, string status, CancellationToken ct)
        {
            //Simulacao de status baseado no valor da doacao, apenas para fins de teste.
            // Par - Aprovado | Ímpar - Recusado
            status = await CheckStatus(valor);


            if (_applicationType == "LOCAL")
            {
                await SendDonationCreatedEventMessageRabbit(guidUser, nome, email, guidCampanha, tituloCampanha, cpf, valor, status, ct);
            }
            else if (_applicationType == "LAB")
            {
                await SendDonationCreatedEventMessageSQS(guidUser, nome, email, guidCampanha, tituloCampanha, cpf, valor, status, ct);
            }

        }





        // -----------------------------------------------------------------------------
        // Privados
        // -----------------------------------------------------------------------------
        private async Task<string> CheckStatus(decimal valor)
        {
            // Converte a parte inteira do decimal para long/int para aplicar a verificação de par/ímpar
            long valorInteiro = (long)Math.Truncate(valor);

            // Regra: Par (incluindo 0) = 1 (Aprovado) | Ímpar = 2 (Recusado)
            // Math.Abs garante que não quebre mesmo se por algum motivo vier valor negativo
            string status = (Math.Abs(valorInteiro) % 2 == 0) ? DoacaoStatus.APROVADA.ToString() : DoacaoStatus.RECUSADA.ToString();
            return status;
        }




        //Rabbit
        private async Task SendDonationCreatedEventMessageRabbit(Guid guidUser, string nome, string email, Guid guidCampanha, string tituloCampanha, string cpf, decimal valor, string status, CancellationToken ct)
        {

            try
            {
                var eventMessage = new DonationCreatedEvent(guidUser, nome, email, guidCampanha, tituloCampanha, cpf, valor, status, _correlationIdGenerator.Get());
                await _publish.Publish(eventMessage, ct);
                _logger.LogInformation("Evento DonationCreatedEvent publicado para o Broker. Email: {Email}", BaseLogType.EVENT, eventMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao publicar evento DonationCreatedEvent para o Broker. Email: {Email}", BaseLogType.EVENT, ex, new { email });
                throw;
            }

        }



        //SQS
        private async Task SendDonationCreatedEventMessageSQS(Guid guidUser, string nome, string email, Guid guidCampanha, string tituloCampanha, string cpf, decimal valor, string status, CancellationToken ct)
        {
            var message = new
            {
                guidUser = guidUser.ToString(),
                nome = nome,
                email = email,
                guidCampanha = guidCampanha.ToString(),
                tituloCampanha = tituloCampanha,
                cpf = cpf,
                valor = valor,
                status = status,
                correlationId = _correlationIdGenerator.Get()
            };

            try
            {
                var messageBody = JsonSerializer.Serialize(message);
                var response = await _sqsClient.SendMessageAsync(new SendMessageRequest
                {
                    QueueUrl = _donationCreatedQueueUrl,
                    MessageBody = messageBody
                });
                _logger.LogInformation("Evento DonationCreatedEvent publicado para o SQS. Email: {Email}", BaseLogType.EVENT, message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao publicar evento DonationCreatedEvent para o SQS. Email: {Email}", BaseLogType.EVENT, ex, new { email });
                throw;
            }

        }
    }
}