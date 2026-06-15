using Campanhas.Application.Shared;
using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Entities.Campanhas.Enums;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Exceptions;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Primitives;

namespace Campanhas.Application.Features.Campanhas;

public sealed class CriarDoacaoCommandHandler : IUseCaseHandler<CriarDoacaoCommand, Result<CriarDoacaoResponse>>
{
    private readonly ICampanhaRepository _campanhaRepository;
    private readonly IUserContext _userContext;
    private readonly IBaseLogger<AlterarCampanhaCommandHandler> _logger;
    private readonly IMessageService _messageService;
    private readonly ICacheService _cacheService;
    private readonly IMetricsService _metrics;
    private readonly IElasticSearchService _elasticSearchService;
    private readonly ICryptoService _cryptoService;

    public CriarDoacaoCommandHandler(ICampanhaRepository campanhaRepository, IUserContext userContext,
        IBaseLogger<AlterarCampanhaCommandHandler> logger, IMessageService messageService,
        ICacheService cacheService, IMetricsService metrics, IElasticSearchService elasticSearchService,
        ICryptoService cryptoService)
    {
        _campanhaRepository = campanhaRepository;
        _userContext = userContext;
        _logger = logger;
        _messageService = messageService;
        _cacheService = cacheService;
        _metrics = metrics;
        _elasticSearchService = elasticSearchService;
        _cryptoService = cryptoService;
    }

    public async Task<Result<CriarDoacaoResponse>> HandleAsync(CriarDoacaoCommand command, CancellationToken ct = default)
    {
        // 1 - Verificar se o command não é nulo 
        if (command == null)
        {
            throw new DomainException("400_COMMAND_INVALID");
        }

        try
        {
            _logger.LogInformation("Tentativa de doacao iniciada para campanha com o Guid: " + command.Guid, BaseLogType.LOG, command);

            // 2 - Buscar solicitante
            var solicitante = _userContext.GetUser() ?? null;

            if (solicitante == null)
            {
                throw new DomainException("400_REQUESTER_REQUIRED");
            }

            // 3 - Verificar se campanha existe.
            var campanhaExistente = await _campanhaRepository.ObterPorGuidAsync(command.Guid, ct);

            if (campanhaExistente == null)
            {
                throw new DomainException("400_CAMPAIGN_DOES_NOT_EXIST");
            }

            if (campanhaExistente.StatusCampanha != CampanhaStatus.ATIVA)
            {
                throw new DomainException("403_CAMPAIGN_DOES_NOT_ACCEPT_DONATION");
            }
            Console.WriteLine(solicitante.Cpf);
            // 4 - Enviar o evento de intenção de doacao que sera consumido pelo worker
            
            await _messageService.SendDonationCreatedEventMessage(solicitante.Guid, solicitante.NomeCompleto,
                solicitante.Email, campanhaExistente.Guid, campanhaExistente.Titulo, solicitante.Cpf, command.Valor, ct);

            // ── Métrica de negócio ─────────────────────────────────────────
            _metrics.IncrementarIntencaoDoacao();

            var response = new CriarDoacaoResponse(
                campanhaExistente.Guid,
                campanhaExistente.Titulo,
                solicitante.NomeCompleto,
                solicitante.Email,
                solicitante.Cpf,
                command.Valor
                );

            return Result<CriarDoacaoResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao realizar doação: " + ex.Message, BaseLogType.LOG, ex.Message);
            return Result<CriarDoacaoResponse>.Failure(ex.Message);
        }
    }
}
