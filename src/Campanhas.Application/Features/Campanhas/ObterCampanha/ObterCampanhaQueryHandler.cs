using Campanhas.Application.Shared;
using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Entities.Campanhas.Enums;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Exceptions;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Primitives;
using Microsoft.Extensions.Configuration;

namespace Campanhas.Application.Features.Campanhas
{
    public sealed class ObterCampanhaQueryHandler : IUseCaseHandler<ObterCampanhaQuery, Result<ObterCampanhaResponse>>
    {
        private readonly ICampanhaRepository _campanhaRepository;
        private readonly IUserContext _userContext;
        private readonly IBaseLogger<ObterCampanhaQueryHandler> _logger;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public ObterCampanhaQueryHandler(ICampanhaRepository campanhaRepository, IUserContext userContext,
            IBaseLogger<ObterCampanhaQueryHandler> logger, ICacheService cacheService, IConfiguration configuration)
        {
            _campanhaRepository = campanhaRepository;
            _userContext = userContext;
            _logger = logger;
            _cacheService = cacheService;
            _configuration = configuration;
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
                _logger.LogInformation("Tentativa de busca de campanha iniciada para o guid: {Guid}", BaseLogType.LOG, new { Guid = command.Guid });

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
                    _logger.LogInformation("Campanha nao encontrada no cache, buscando no banco: {Guid}", BaseLogType.LOG, new { Guid = command.Guid });

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
                    _logger.LogInformation("Campanha encontrada no cache: {Guid}", BaseLogType.LOG, new { Guid = command.Guid });
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
                _logger.LogError("Erro ao obter campanha: {ExceptionMsg}", BaseLogType.LOG, ex);
                throw new ApplicationException("Ocorreu um erro ao obter a campanha. " + ex.Message);
            }
        }
    }
}
