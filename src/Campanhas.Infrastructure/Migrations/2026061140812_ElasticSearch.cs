// Campanhas.Infrastructure/Migrations/ElasticSearchMigration.cs
using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Interfaces;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;

public static class ElasticSearchMigration
{
    public static async Task ElasticMigration(IServiceProvider services)
    {
        var elasticService = services.GetRequiredService<IElasticSearchService>();
        var campanhaRepository = services.GetRequiredService<ICampanhaRepository>();
        var logger = services.GetRequiredService<IBaseLogger<ElasticsearchClient>>();

        try
        {
            // 1. Verifica se o índice já existe
            var indexExists = await elasticService.IndexExistsAsync("campanha");

            if (!indexExists)
            {
                logger.LogInformation("Indice 'campanha' não encontrado — criando e indexando...", BaseLogType.LOG, null);

                // 2. Busca todas as campanhas do banco
                var campanhas = await campanhaRepository.ObterTodosAsync(CancellationToken.None);

                // 3. Indexa cada uma
                foreach (var campanha in campanhas)
                {
                    var dto = CampanhaDTO.FromEntity(campanha);
                    await elasticService.IndexAsync(dto);
                }

                logger.LogInformation($"{campanhas.Count} campanhas indexadas no Elasticsearch.", BaseLogType.LOG, null);
            }
            else
            {
                logger.LogInformation("Índice 'campanha' já existe — nenhuma ação necessária.", BaseLogType.LOG, null);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Erro ao executar ElasticSearch migration: " + ex.Message, BaseLogType.LOG, ex);
            throw;
        }
    }
}