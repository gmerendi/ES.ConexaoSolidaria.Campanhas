using Campanhas.Application.Shared;
using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Exceptions;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Primitives;

namespace Campanhas.Application.Features.Campanhas
{
    public sealed class ObterCampanhaQueryHandler : IUseCaseHandler<ObterCampanhaQuery, Result<ObterCampanhaResponse>>
    {
        private readonly ICampanhaRepository _campanhaRepository;
        private readonly IUserContext _userContext;
        private readonly IBaseLogger<ObterCampanhaQueryHandler> _logger;
        private readonly ICacheService _cacheService;

        public ObterCampanhaQueryHandler(ICampanhaRepository campanhaRepository, IUserContext userContext,
            IBaseLogger<ObterCampanhaQueryHandler> logger, ICacheService cacheService)
        {
            _campanhaRepository = campanhaRepository;
            _userContext = userContext;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<Result<ObterCampanhaResponse>> HandleAsync(ObterCampanhaQuery command, CancellationToken ct)
        {
            // 1 - Verificar se o command não é nulo 
            if (command == null)
            {
                throw new DomainException("400_COMMAND_INVALID");
            }

            try
            {
                _logger.LogInformation("Tentativa de busca de campanha iniciada para o guid: " + command.Guid, BaseLogType.LOG, command);

                //2 - Buscar solicitante. 
                var solicitante = _userContext.GetUser() ?? null;

                if (solicitante == null)
                {
                    throw new DomainException("400_REQUESTER_REQUIRED");
                }


                // 3 - Tentar obter campanha do cache
                var cacheKey = $"campanha:{command.Guid}";
                var campanha = await _cacheService.GetAsync<CampanhaDTO>(cacheKey);


                // 4 - Buscar campanha - não encontrado no cache
                if (campanha == null)
                {
                    _logger.LogInformation("Campanha não encontrada no cache, buscando no banco: " + command.Guid, BaseLogType.LOG, command);

                    var campanhaDb = await _campanhaRepository.ObterPorGuidAsync(command.Guid, ct);
                    if (campanhaDb == null)
                    {
                        throw new DomainException("400_CAMPAIGN_NOT_FOUND");
                    }
                    // 5 - Insere campanha no cache
                    campanha = CampanhaDTO.FromEntity(campanhaDb);
                    await _cacheService.SetAsync(cacheKey, campanha, TimeSpan.FromMinutes(30));
                }
                else
                {
                    _logger.LogInformation("Campanha encontrado no cache: " + command.Guid, BaseLogType.LOG, command);
                }

                var response = new ObterCampanhaResponse
                (
                    campanha.Guid,
                    campanha.Titulo,
                    campanha.Descricao,
                    campanha.MetaFinanceira,
                    campanha.ValorArrecadado,
                    campanha.DataInicio,
                    campanha.DataFim,
                    campanha.StatusCampanha
                );
                return Result<ObterCampanhaResponse>.Success(response);
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao obter campanha: " + ex.Message, BaseLogType.LOG, ex.Message);
                throw new ApplicationException("Ocorreu um erro ao obter a campanha. " + ex.Message);
            }
        }
    }
}
