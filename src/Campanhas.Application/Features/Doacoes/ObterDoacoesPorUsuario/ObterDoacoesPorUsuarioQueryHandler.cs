using Campanhas.Application.Shared;
using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Entities.Campanhas.Enums;
using Campanhas.Domain.Entities.Doacoes;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Exceptions;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Primitives;
using Campanhas.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace Campanhas.Application.Features.Campanhas
{
    public sealed class ObterDoacoesPorUsuarioQueryHandler : IUseCaseHandler<ObterDoacoesPorUsuarioQuery, Result<ObterDoacoesPorUsuarioResponse>>
    {
        private readonly ICampanhaRepository _campanhaRepository;
        private readonly IDoacaoRepository _doacaoRepository;
        private readonly IBaseLogger<ObterDoacoesPorUsuarioQueryHandler> _logger;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public ObterDoacoesPorUsuarioQueryHandler(ICampanhaRepository campanhaRepository,
            IBaseLogger<ObterDoacoesPorUsuarioQueryHandler> logger, ICacheService cacheService, IConfiguration configuration,
            IDoacaoRepository doacaoRepository)
        {
            _campanhaRepository = campanhaRepository;
            _logger = logger;
            _cacheService = cacheService;
            _configuration = configuration;
            _doacaoRepository = doacaoRepository;
        }

        public async Task<Result<ObterDoacoesPorUsuarioResponse>> HandleAsync(ObterDoacoesPorUsuarioQuery command, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Tentativa de busca de todas as doacoes do usuario {Email}", BaseLogType.LOG, new { command.Email });

                // 1 - Usuario nao existe nesse servico.  Procura direto por email


                // Busca doacoes para o referido usuario
                var emailUsuario = Email.Create(command.Email);
                var listaDoacoes = await _doacaoRepository.ObterPorUsuarioAsync(emailUsuario, ct);


                return Result<ObterDoacoesPorUsuarioResponse>.Success(new ObterDoacoesPorUsuarioResponse(listaDoacoes));
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