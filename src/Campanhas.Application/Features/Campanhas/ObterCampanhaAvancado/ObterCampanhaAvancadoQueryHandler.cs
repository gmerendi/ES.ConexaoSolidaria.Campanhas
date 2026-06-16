using Campanhas.Application.Shared;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Exceptions;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Primitives;

namespace Campanhas.Application.Features.Campanhas
{
    public sealed class ObterCampanhaAvancadoQueryHandler : IUseCaseHandler<ObterCampanhaAvancadoQuery, Result<ObterCampanhaAvancadoResponse>>
    {
        private readonly IUserContext _userContext;
        private readonly IBaseLogger<ObterCampanhaAvancadoQueryHandler> _logger;
        private readonly IElasticSearchService _elasticSearchService;

        public ObterCampanhaAvancadoQueryHandler(IUserContext userContext,IBaseLogger<ObterCampanhaAvancadoQueryHandler> logger,
            IElasticSearchService elasticSearchService)
        {
            _userContext = userContext;
            _logger = logger;
            _elasticSearchService = elasticSearchService;
        }

        public async Task<Result<ObterCampanhaAvancadoResponse>> HandleAsync(ObterCampanhaAvancadoQuery command, CancellationToken ct)
        {
            // 1 - Verificar se o command não é nulo 
            if (command == null)
            {
                throw new DomainException("400_COMMAND_INVALID");
            }

            try
            {
                _logger.LogInformation("Tentativa de busca de campanha avançada iniciada para o termo: " + command.Termo, BaseLogType.LOG, command);

                //2 - Buscar solicitante. 
                var solicitante = _userContext.GetUser() ?? null;

                if (solicitante == null)
                {
                    throw new DomainException("400_REQUESTER_REQUIRED");
                }


                // 3 - Busca no elasticSearch
                var response = await _elasticSearchService.SearchAsync(command.Termo);

                return Result<ObterCampanhaAvancadoResponse>.Success(new ObterCampanhaAvancadoResponse(response));
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
