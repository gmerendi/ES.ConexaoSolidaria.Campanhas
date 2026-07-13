using Campanhas.Application.Shared;
using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Entities.Campanhas.Enums;
using Campanhas.Domain.Entities.Doacoes;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Exceptions;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Primitives;
using Microsoft.Extensions.Configuration;

namespace Campanhas.Application.Features.Campanhas
{
    public sealed class ObterDoacoesPorCampanhaQueryHandler : IUseCaseHandler<ObterDoacoesPorCampanhaQuery, Result<ObterDoacoesPorCampanhaResponse>>
    {
        private readonly ICampanhaRepository _campanhaRepository;
        private readonly IDoacaoRepository _doacaoRepository;
        private readonly IBaseLogger<ObterDoacoesPorCampanhaQueryHandler> _logger;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public ObterDoacoesPorCampanhaQueryHandler(ICampanhaRepository campanhaRepository,
            IBaseLogger<ObterDoacoesPorCampanhaQueryHandler> logger, ICacheService cacheService, IConfiguration configuration,
            IDoacaoRepository doacaoRepository)
        {
            _campanhaRepository = campanhaRepository;
            _logger = logger;
            _cacheService = cacheService;
            _configuration = configuration;
            _doacaoRepository = doacaoRepository;
        }

        public async Task<Result<ObterDoacoesPorCampanhaResponse>> HandleAsync(ObterDoacoesPorCampanhaQuery command, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Tentativa de busca de todas as doacoes para a campanha {Guid}", BaseLogType.LOG, new { command.Guid });

                // 1 - Tentar obter campanha do cache
                var cacheKey = $"campanha:{command.Guid}";
                var campanha = await _cacheService.GetAsync<CampanhaDTO>(cacheKey);

                // 2 - Buscar campanha - não encontrado no cache
                if (campanha == null)
                {
                    _logger.LogInformation("Campanha não encontrada no cache, buscando no banco: {Guid}", BaseLogType.LOG, new { command.Guid });

                    var campanhaDb = await _campanhaRepository.ObterPorGuidAsync(command.Guid, ct);
                    if (campanhaDb == null)
                    {
                        throw new DomainException("400_CAMPAIGN_NOT_FOUND");
                    }
                    // 5 - Insere campanha no cache
                    campanha = CampanhaDTO.FromEntity(campanhaDb);
                    var ttlAtiva = _configuration.GetValue<int>("Cache:CampanhaAtivaTTLSeconds");
                    var ttlNaoAtiva = _configuration.GetValue<int>("Cache:CampanhaAtivaTTLSeconds");
                    var ttl = campanhaDb.StatusCampanha == CampanhaStatus.ATIVA ? ttlAtiva : ttlNaoAtiva;
                    await _cacheService.SetAsync(cacheKey, campanha, TimeSpan.FromSeconds(ttl));
                }
                else
                {
                    _logger.LogInformation("Campanha encontrado no cache: {Guid}", BaseLogType.LOG, new { command.Guid });
                }

                // Busca doacoes para a referida campanha
                var listaDoacoes = await _doacaoRepository.ObterPorCampanhaAsync(command.Guid, ct);


                return Result<ObterDoacoesPorCampanhaResponse>.Success(new ObterDoacoesPorCampanhaResponse(listaDoacoes));
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao obter as doacoes: {ExceptionMsg}", BaseLogType.LOG, ex);
                throw new ApplicationException("Ocorreu um erro ao obter as doacoes. " + ex.Message);
            }
        }
    }
}