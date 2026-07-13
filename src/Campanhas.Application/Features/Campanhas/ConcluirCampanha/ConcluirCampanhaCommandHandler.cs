using Campanhas.Application.Shared;
using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Entities.Campanhas.Enums;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Exceptions;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Primitives;

namespace Campanhas.Application.Features.Campanhas
{
    public sealed class ConcluirCampanhaCommandHandler : IUseCaseHandler<ConcluirCampanhaCommand, Result<bool>>
    {
        private readonly ICampanhaRepository _campanhaRepository;
        private readonly IUserContext _userContext;
        private readonly IBaseLogger<ConcluirCampanhaCommandHandler> _logger;
        private readonly ICacheService _cacheService;
        private readonly IMetricsService _metrics;
        private readonly IElasticSearchService _elasticSearchService;

        public ConcluirCampanhaCommandHandler(ICampanhaRepository campanhaRepository, IUserContext userContext,
            IBaseLogger<ConcluirCampanhaCommandHandler> logger, ICacheService cacheService,
            IMetricsService metrics, IElasticSearchService elasticSearchService)
        {
            _campanhaRepository = campanhaRepository;
            _userContext = userContext;
            _logger = logger;
            _cacheService = cacheService;
            _metrics = metrics;
            _elasticSearchService = elasticSearchService;
        }

        public async Task<Result<bool>> HandleAsync(ConcluirCampanhaCommand command, CancellationToken ct)
        {
            // 1 - Verificar se o command não é nulo 
            if (command == null)
            {
                throw new DomainException("400_COMMAND_INVALID");
            }

            try
            {
                _logger.LogInformation("Tentativa de conclusão de campanha: {Guid}", BaseLogType.LOG, new { command.Guid });

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
                campanha.ConcluirCampanha(solicitante.Email);
                await _campanhaRepository.AlterarAsync(campanha);
                await _cacheService.RemoveByPrefixAsync("campanhas:ativas");


                // 5 - Remove campanha do cache
                var cacheKey = $"campanha:{command.Guid}";
                await _cacheService.RemoveAsync(cacheKey);

                // 6 - Faz update no ElasticSearch
                var campanhaDTO = CampanhaDTO.FromEntity(campanha);
                await _elasticSearchService.UpdateAsync(campanhaDTO);

                // ── Métrica de negócio ─────────────────────────────────────────
                _metrics.IncrementarCampanhaConcluida();

                return Result<bool>.Success(true);
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao concluir campanha: {ExceptionMsg}", BaseLogType.LOG, ex);
                throw new ApplicationException("Ocorreu um erro ao concluir campanha. " + ex.Message);
            }
        }
    }
}