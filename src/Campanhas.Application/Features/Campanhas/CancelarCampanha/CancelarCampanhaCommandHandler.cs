using Campanhas.Application.Shared;
using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Entities.Campanhas.Enums;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Exceptions;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Primitives;

namespace Campanhas.Application.Features.Campanhas
{
    public sealed class CancelarCampanhaCommandHandler : IUseCaseHandler<CancelarCampanhaCommand, Result<bool>>
    {
        private readonly ICampanhaRepository _campanhaRepository;
        private readonly IUserContext _userContext;
        private readonly IBaseLogger<CancelarCampanhaCommandHandler> _logger;
        private readonly ICacheService _cacheService;
        private readonly IMetricsService _metrics;
        private readonly IElasticSearchService _elasticSearchService;

        public CancelarCampanhaCommandHandler(ICampanhaRepository campanhaRepository, IUserContext userContext,
            IBaseLogger<CancelarCampanhaCommandHandler> logger, ICacheService cacheService,
            IMetricsService metrics, IElasticSearchService elasticSearchService)
        {
            _campanhaRepository = campanhaRepository;
            _userContext = userContext;
            _logger = logger;
            _cacheService = cacheService;
            _metrics = metrics;
            _elasticSearchService = elasticSearchService;
        }

        public async Task<Result<bool>> HandleAsync(CancelarCampanhaCommand command, CancellationToken ct)
        {
            // 1 - Verificar se o command não é nulo 
            if (command == null)
            {
                throw new DomainException("400_COMMAND_INVALID");
            }

            try
            {
                _logger.LogInformation("Tentativa de cancelamento de campanha: {Guid}", BaseLogType.LOG, new { command.Guid });

                //2 - Buscar solicitante.             
                var solicitante = _userContext.GetUser() ?? null;

                if (solicitante == null)
                {
                    throw new DomainException("400_REQUESTER_REQUIRED");
                }

                if (solicitante.Perfil != Perfil.GESTOR_ONG.ToString())
                {
                    throw new DomainException("403_USER_NOT_ALLOWED");
                }


                //3 - Buscar campanha
                var campanha = await _campanhaRepository.ObterPorGuidAsync(command.Guid);

                if (campanha == null)
                {
                    throw new DomainException("400_CAMPAIGN_NOT_FOUND");
                }

                //4 - A campanha precisa estar ATIVA
                if (campanha.StatusCampanha != CampanhaStatus.ATIVA)
                {
                    throw new DomainException("422_CAMPAIGN_NOT_ACTIVE");
                }


                // 3 - Modifica o status para CANCELADA
                campanha.CancelarCampanha(solicitante.Email);
                await _campanhaRepository.AlterarAsync(campanha);


                // 5 - Remove campanha do cache
                var cacheKey = $"campanha:{command.Guid}";
                await _cacheService.RemoveAsync(cacheKey);
                await _cacheService.RemoveByPrefixAsync("campanhas:ativas");

                // 6 - Faz update no ElasticSearch
                var campanhaDTO = CampanhaDTO.FromEntity(campanha);
                await _elasticSearchService.UpdateAsync(campanhaDTO);

                // ── Métrica de negócio ─────────────────────────────────────────
                _metrics.IncrementarCampanhaCancelada();

                return Result<bool>.Success(true);
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao cancelar campanha: {ExceptionMsg}", BaseLogType.LOG, ex);
                throw new ApplicationException("Ocorreu um erro ao cancelar campanha. " + ex.Message);
            }
        }
    }
}