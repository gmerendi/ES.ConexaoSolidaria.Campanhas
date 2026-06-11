using Campanhas.Application.Shared;
using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Exceptions;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Primitives;

namespace Campanhas.Application.Features.Campanhas
{
    public sealed class ObterTodasCampanhasQueryHandler : IUseCaseHandler<ObterTodasCampanhasQuery, Result<ObterTodasCampanhasResponse>>
    {
        private readonly ICampanhaRepository _campanhaRepository;
        private readonly IUserContext _userContext;
        private readonly IBaseLogger<ObterTodasCampanhasQueryHandler> _logger;
        private readonly ICacheService _cacheService;

        public ObterTodasCampanhasQueryHandler(ICampanhaRepository campanhaRepository, IUserContext userContext,
            IBaseLogger<ObterTodasCampanhasQueryHandler> logger, ICacheService cacheService)
        {
            _campanhaRepository = campanhaRepository;
            _userContext = userContext;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<Result<ObterTodasCampanhasResponse>> HandleAsync(ObterTodasCampanhasQuery command, CancellationToken ct)
        {        
            try
            {
                _logger.LogInformation("Tentativa de busca de todas as campanhas ", BaseLogType.LOG, command);

                // 1 - Tentar obter campanha do cache
                var cacheKey = $"campanhas:ativas:pagina:{command.Pagina}:tamanho:{command.TamanhoPagina}";
                var listaCampanhas = await _cacheService.GetAsync<List<CampanhaDTO>>(cacheKey);


                // 2 - Buscar campanhas - não encontrado no cache
                if (listaCampanhas == null)
                {
                    _logger.LogInformation("Campanhas ativas nao encontradas no cache, buscando no banco. ", BaseLogType.LOG, command);

                    var campanhaDbList = await _campanhaRepository.ObterTodosAsync(command.Pagina, command.TamanhoPagina, ct);


                    if (campanhaDbList == null || !campanhaDbList.Any())
                    {
                        throw new DomainException("400_CAMPAIGN_NONE_ACTIVE");
                    }

                    listaCampanhas = campanhaDbList.Select(c => CampanhaDTO.FromEntity(c)).ToList();

                    // 5 - Insere lista de campanhas no cache
                    await _cacheService.SetAsync(cacheKey, listaCampanhas, TimeSpan.FromMinutes(30));
                }
                else
                {
                    _logger.LogInformation("Campanhas ativas encontradas no cache. ", BaseLogType.LOG, listaCampanhas);
                }


                return Result<ObterTodasCampanhasResponse>.Success(new ObterTodasCampanhasResponse(listaCampanhas));
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao obter usuario: " + ex.Message, BaseLogType.LOG, ex.Message);
                throw new ApplicationException("Ocorreu um erro ao obter o usuário. " + ex.Message);
            }
        }
    }
}
