using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Infrastructure.Services.ElasticSearch;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class ElasticSearchExtension
{
    public static IServiceCollection AddElasticSearch(this IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        var url = configuration.GetConnectionString("Elasticsearch")
               ?? Environment.GetEnvironmentVariable("ConnectionStrings__Elasticsearch")
               ?? "http://localhost:9200";

        var settings = new ElasticsearchClientSettings(new Uri(url))
            .DefaultIndex("campanha");

        var client = new ElasticsearchClient(settings);

        services.AddSingleton(client);
        services.AddSingleton<IElasticSearchService, ElasticSearchService>();
        services.AddHostedService<ElasticSearchSeedService>();

        logger.LogInformation(" ***** ElasticSearch inicializado: " + url);

        return services;
    }

}