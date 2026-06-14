using Campanhas.Application.Shared;
using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Entities.ValueObjects;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Exceptions;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Primitives;

namespace Campanhas.Application.Features.Campanhas;

public sealed class AlterarCampanhaCommandHandler : IUseCaseHandler<AlterarCampanhaCommand, Result<AlterarCampanhaResponse>>
{
    private readonly ICampanhaRepository _campanhaRepository;
    private readonly IUserContext _userContext;
    private readonly IBaseLogger<AlterarCampanhaCommandHandler> _logger;
    private readonly IMessageService _messageService;
    private readonly ICacheService _cacheService;
    private readonly IMetricsService _metrics;
    private readonly IElasticSearchService _elasticSearchService;

    public AlterarCampanhaCommandHandler(ICampanhaRepository campanhaRepository, IUserContext userContext,
        IBaseLogger<AlterarCampanhaCommandHandler> logger, IMessageService messageService,
        ICacheService cacheService, IMetricsService metrics, IElasticSearchService elasticSearchService)
    {
        _campanhaRepository = campanhaRepository;
        _userContext = userContext;
        _logger = logger;
        _messageService = messageService;
        _cacheService = cacheService;
        _metrics = metrics;
        _elasticSearchService = elasticSearchService;
    }

    public async Task<Result<AlterarCampanhaResponse>> HandleAsync(AlterarCampanhaCommand command, CancellationToken ct = default)
    {
        // 1 - Verificar se o command não é nulo 
        if (command == null)
        {
            throw new DomainException("400_COMMAND_INVALID");
        }

        try
        {
            _logger.LogInformation("Tentativa de alteracao de campanha iniciada com o titulo: " + command.Titulo, BaseLogType.LOG, command);

            // 2 - Buscar solicitante
            var solicitante = _userContext.GetUser() ?? null;

            if (solicitante == null)
            {
                throw new DomainException("400_REQUESTER_REQUIRED");
            }

            if (solicitante.Perfil != Perfil.GESTOR_ONG.ToString())
            {
                throw new DomainException("403_CAMPAIGN_CAN_BE_CHANGED_BY_MANAGER");
            }

            // 3 - Verificar se campanha existe
            var campanhaExistente = await _campanhaRepository.ObterPorGuidAsync(command.Guid, ct);

            if (campanhaExistente == null)
            {
                throw new DomainException("400_CAMPAIGN_DOES_NOT_EXIST");
            }


            // 4 - Modificar a campanha
            var titulo = TituloCampanha.Create(command.Titulo);
            var metaFinanceira = MetaFinanceira.Create(command.MetaFinanceira);

            campanhaExistente.AlterarCampanha(
                titulo,
                command.Descricao,
                metaFinanceira,
                command.DataInicio,
                command.DataFim,
                solicitante.Email
                );
            await _campanhaRepository.AlterarAsync(campanhaExistente);
           
            // 5 - Remove campanha do cache
            var cacheKey = $"campanha:{campanhaExistente.Guid}";
            await _cacheService.RemoveAsync(cacheKey);
            await _cacheService.RemoveByPrefixAsync("campanhas:ativas");

            // 6 - Faz update no ElasticSearch
            var campanhaDTO = CampanhaDTO.FromEntity(campanhaExistente);
            await _elasticSearchService.UpdateAsync(campanhaDTO);

            CampanhaDTO campanhaResponse = CampanhaDTO.FromEntity(campanhaExistente);
            var response = new AlterarCampanhaResponse(
                campanhaResponse.Guid,
                campanhaResponse.Titulo,
                campanhaResponse.Descricao,
                campanhaResponse.MetaFinanceira,
                campanhaResponse.ValorArrecadado,
                campanhaResponse.DataInicio,
                campanhaResponse.DataFim,
                campanhaResponse.StatusCampanha
                );

            return Result<AlterarCampanhaResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao alterar campanha: " + ex.Message, BaseLogType.LOG, ex.Message);
            return Result<AlterarCampanhaResponse>.Failure(ex.Message);
        }
    }
}
