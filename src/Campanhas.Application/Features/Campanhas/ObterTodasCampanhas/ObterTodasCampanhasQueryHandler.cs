using Campanhas.Application.Shared;
using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Exceptions;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Primitives;
using Microsoft.Extensions.Configuration;

namespace Campanhas.Application.Features.Campanhas
{
    public sealed class ObterTodasCampanhasQueryHandler : IUseCaseHandler<ObterTodasCampanhasQuery, Result<ObterTodasCampanhasResponse>>
    {
        private readonly ICampanhaRepository _campanhaRepository;
        private readonly IBaseLogger<ObterTodasCampanhasQueryHandler> _logger;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public ObterTodasCampanhasQueryHandler(ICampanhaRepository campanhaRepository,
            IBaseLogger<ObterTodasCampanhasQueryHandler> logger, ICacheService cacheService, IConfiguration configuration)
        {
            _campanhaRepository = campanhaRepository;
            _logger = logger;
            _cacheService = cacheService;
            _configuration = configuration;
        }

        public async Task<Result<ObterTodasCampanhasResponse>> HandleAsync(ObterTodasCampanhasQuery command, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Tentativa de busca de todas as campanhas.", BaseLogType.LOG, new { Pagina = command.Pagina, TamanhoPagina = command.TamanhoPagina });

                // 1 - Tentar obter campanha do cache
                var cacheKey = $"campanhas:ativas:pagina:{command.Pagina}:tamanho:{command.TamanhoPagina}";
                var listaCampanhas = await _cacheService.GetAsync<List<CampanhaDTO>>(cacheKey);


                // 2 - Buscar campanhas - não encontrado no cache
                if (listaCampanhas == null)
                {
                    _logger.LogInformation("Campanhas ativas nao encontradas no cache, buscando no banco.", BaseLogType.LOG, new { Pagina = command.Pagina, TamanhoPagina = command.TamanhoPagina });

                    var campanhaDbList = await _campanhaRepository.ObterTodosAsync(command.Pagina, command.TamanhoPagina, ct);


                    if (campanhaDbList == null || !campanhaDbList.Any())
                    {
                        throw new DomainException("400_CAMPAIGN_NONE_ACTIVE");
                    }

                    listaCampanhas = campanhaDbList.Select(c => CampanhaDTO.FromEntity(c)).ToList();

                    // 5 - Insere lista de campanhas no cache.  Pega sempre o ttl de ativas.
                    var ttlAtiva = _configuration.GetValue<int>("Cache:CampanhaAtivaTTLSeconds");
                    await _cacheService.SetAsync(cacheKey, listaCampanhas, TimeSpan.FromSeconds(ttlAtiva));
                }
                else
                {
                    _logger.LogInformation("Campanhas ativas encontradas no cache.", BaseLogType.LOG, new { Total = listaCampanhas.Count });
                }


                return Result<ObterTodasCampanhasResponse>.Success(new ObterTodasCampanhasResponse(listaCampanhas));
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao obter as campanhas: {ExceptionMsg}", BaseLogType.LOG, ex);
                throw new ApplicationException("Ocorreu um erro ao obter as campanhas. " + ex.Message);
            }
        }
    }
}