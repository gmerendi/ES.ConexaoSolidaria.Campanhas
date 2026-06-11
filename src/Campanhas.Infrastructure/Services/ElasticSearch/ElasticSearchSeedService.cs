using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Campanhas.Infrastructure.Services.ElasticSearch
{
    public class ElasticSearchSeedService : IHostedService
    {
        private readonly IBaseLogger<ElasticSearchSeedService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ElasticSearchSeedService(IBaseLogger<ElasticSearchSeedService> logger,
           IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Verificando necessidade de sincronização do Elasticsearch...", BaseLogType.LOG, null);

                using var scope = _scopeFactory.CreateScope();
                var campanhaRepository = scope.ServiceProvider.GetRequiredService<ICampanhaRepository>();
                var elasticSearchService = scope.ServiceProvider.GetRequiredService<IElasticSearchService>();

                var campanhas = await campanhaRepository.ObterTodosAsync();

                if (campanhas == null || campanhas.Count == 0)
                {
                    _logger.LogInformation("Nenhuma campanha encontrada no banco de dados. Sincronização ignorada.", BaseLogType.LOG, null);
                    return;
                }

                // Verifica se o índice existe antes de contar
                var indexExists = await elasticSearchService.IndexExistsAsync("campanha");

                long totalNoElastic = 0;

                if (indexExists)
                {
                    var countResponse = await elasticSearchService.CountAsync();

                    totalNoElastic = countResponse;
                }
                else
                {
                    _logger.LogInformation("Índice 'campanha' não existe no Elasticsearch. Será criado na indexação.", BaseLogType.LOG, null);
                }

                var totalNoBanco = campanhas.Count;

                _logger.LogInformation("Banco de dados: " + totalNoBanco + " campanhas | Elasticsearch: " + totalNoElastic + " documentos.", BaseLogType.LOG, null);

                if (totalNoElastic >= totalNoBanco)
                {
                    _logger.LogInformation("Elasticsearch já está sincronizado. Sincronização ignorada.", BaseLogType.LOG, null);
                    return;
                }

                _logger.LogInformation("Inconsistência detectada. Iniciando reindexação de jogos.", BaseLogType.LOG, totalNoBanco);

                // Só limpa se o índice existir
                if (indexExists)
                    await ClearIndiceAsync(cancellationToken);

                var indexados = 0;
                var erros = 0;

                await elasticSearchService.CreateIndexIfNotExistsAsync("campanha");

                foreach (var campanha in campanhas)
                {
                    try
                    {
                        var campanhaFull = await campanhaRepository.ObterPorGuidAsync(campanha.Guid);
                        var campanhaDTO = CampanhaDTO.FromEntity(campanhaFull);
                        await elasticSearchService.IndexAsync(campanhaDTO);
                        indexados++;
                    }
                    catch (Exception ex)
                    {
                        erros++;
                        _logger.LogError("Erro ao indexar campanha: " + campanha.Titulo, BaseLogType.LOG, ex.Message);
                    }
                }

                _logger.LogInformation("Sincronização DB e ElasticSearch concluída. Indexados: " + indexados + " | Erros: " + erros, BaseLogType.LOG, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro crítico na sincronização do Elasticsearch.", BaseLogType.LOG, ex.Message);
            }
        }





        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;





        /// <summary>
        /// Apaga todos os documentos do índice antes de reindexar,
        /// evitando duplicatas em caso de reindexação completa.
        /// </summary>
        private async Task ClearIndiceAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var catalogSearchService = scope.ServiceProvider.GetRequiredService<IElasticSearchService>();
            try
            {
                var deleteResponse = await catalogSearchService.ClearIndiceAsync("campanha");
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao limpar índice antes da reindexação: ", BaseLogType.LOG,  ex.Message);
            }
        }
    }
}
