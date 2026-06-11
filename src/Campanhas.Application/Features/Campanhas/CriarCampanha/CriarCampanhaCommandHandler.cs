using Campanhas.Application.Shared;
using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Exceptions;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Primitives;

namespace Campanhas.Application.Features.Campanhas;

public sealed class CriarCampanhaCommandHandler : IUseCaseHandler<CriarCampanhaCommand, Result<CriarCampanhaResponse>>
{
    private readonly ICampanhaRepository _campanhaRepository;
    private readonly IUserContext _userContext;
    private readonly IBaseLogger<CriarCampanhaCommandHandler> _logger;
    private readonly IMessageService _messageService;
    private readonly ICacheService _cacheService;
    private readonly IMetricsService _metrics;

    public CriarCampanhaCommandHandler(ICampanhaRepository campanhaRepository, IUserContext userContext,
        IBaseLogger<CriarCampanhaCommandHandler> logger, IMessageService messageService,
        ICacheService cacheService, IMetricsService metrics)
    {
        _campanhaRepository = campanhaRepository;
        _userContext = userContext;
        _logger = logger;
        _messageService = messageService;
        _cacheService = cacheService;
        _metrics = metrics;

    }

    public async Task<Result<CriarCampanhaResponse>> HandleAsync(CriarCampanhaCommand command, CancellationToken ct = default)
    {
        // 1 - Verificar se o command não é nulo 
        if (command == null)
        {
            throw new DomainException("400_COMMAND_INVALID");
        }

        try
        {
            _logger.LogInformation("Tentativa de criacao de campanha iniciada com o titulo: " + command.Titulo, BaseLogType.LOG, command);
            // 2 - Verificar se campanha já existe
            var campanhaExistente = await _campanhaRepository.ObterPorTituloAsync(command.Titulo, ct);

            if (campanhaExistente != null)
            {
                throw new DomainException("422_CAMPAIGN_DUPLICATED");
            }

            // 3 - Verificar se o solicitante é um usuario logado.
            var solicitante = _userContext.GetUser() ?? null;

            if (solicitante.Perfil != Perfil.GESTOR_ONG.ToString())
            {
                throw new DomainException("403_USER_NOT_ALLOWED");
            }

            // 4 - Criar a campanha
            var titulo = TituloCampanha.Create(command.Titulo);
            var metaFinanceira = MetaFinanceira.Create(command.MetaFinanceira);

            var campanha = new Campanha(
                titulo,
                command.Descricao,
                metaFinanceira,
                command.DataInicio,
                command.DataFim,
                solicitante.Email
                );

            // 5 - Gravar a campanha
            await _campanhaRepository.CadastrarAsync(campanha);

            // 6 - Inserir no cache
            var campanhaDTO = CampanhaDTO.FromEntity(campanha);
            var cacheKey = $"campanha:{campanhaDTO.Guid}";
            await _cacheService.SetAsync(cacheKey, campanhaDTO, TimeSpan.FromMinutes(30));
            await _cacheService.RemoveByPrefixAsync("campanhas:ativas");



            // 7 - Enviar mensagem de campanha criada
            await _messageService.SendCampaignCreatedEventMessage(campanha.Guid, campanha.Titulo, campanha.Descricao,
                campanha.DataInicio, campanha.DataFim, campanha.MetaFinanceira, ct);

            // ── Métrica de negócio ─────────────────────────────────────────
            _metrics.IncrementarCampanhaCriada();

            var campanhaFinal = CampanhaDTO.FromEntity(campanha);
            var response = new CriarCampanhaResponse(
                campanhaFinal.Guid,
                campanhaFinal.Titulo,
                campanhaFinal.Descricao,
                campanhaFinal.MetaFinanceira,
                campanhaFinal.ValorArrecadado,
                campanhaFinal.DataInicio,
                campanhaFinal.DataFim,
                campanhaFinal.StatusCampanha
                );


            return Result<CriarCampanhaResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao cadastrar usuario: " + ex.Message, BaseLogType.LOG, ex.Message);
            return Result<CriarCampanhaResponse>.Failure(ex.Message);
        }
    }
}
