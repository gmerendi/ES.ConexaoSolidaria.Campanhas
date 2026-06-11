using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace Campanhas.Infrastructure.Services.ElasticSearch
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly IBaseLogger<ElasticSearchService> _logger;

        public ElasticSearchService(ElasticsearchClient client, IBaseLogger<ElasticSearchService> logger)
        {
            _client = client;
            _logger = logger;
        }




        public async Task IndexAsync(CampanhaDTO campanha)
        {
            var response = await _client.IndexAsync(campanha, i => i.Index("campanha").Id(campanha.Guid.ToString()));

            if (!response.IsValidResponse)
            {
                throw new Exception($"Falha ao indexar no ElasticSearch: {response.DebugInformation}");
            }
        }




        public async Task<IEnumerable<CampanhaDTO>> SearchAsync(string term)
        {
            _logger.LogInformation($"Inicio de busca no Elasticsearch.", BaseLogType.LOG, term);


            var response = await _client.SearchAsync<CampanhaDTO>(s => s
                .Index("campanha")
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            // MultiMatch fuzzy para erros de digitação
                            sh => sh.MultiMatch(m => m
                                .Fields(new Field[] {
                                    new Field("titulo^3"),
                                    new Field("descricao"),
                                    new Field("statusCampanha"),
                                    new Field("dataInicio"),
                                    new Field("dataFim")
                                })
                                .Query(term)
                                .Fuzziness(new Fuzziness("AUTO")) 
                                .Type(TextQueryType.BestFields)
                            ),
                            // Prefix query para buscas parciais 
                            sh => sh.MultiMatch(m => m
                                .Fields(new Field[] {
                                    new Field("titulo^3"),
                                    new Field("descricao"),
                                    new Field("statusCampanha"),
                                    new Field("dataInicio"),
                                    new Field("dataFim")
                                })
                                .Query(term)
                                .Fuzziness(new Fuzziness("AUTO")) 
                                .Type(TextQueryType.BoolPrefix)  
                            )
                        )
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError($"Erro no Elasticsearch: {response.DebugInformation}", BaseLogType.LOG, response);
            }

            _logger.LogInformation($"Busca no Elasticsearch retornada com sucesso", BaseLogType.LOG,response.Documents);
            return response.Documents;
        }



        public async Task<long> CountAsync()
        {
            var countResponse = await _client.CountAsync<CampanhaDTO>(c => c.Indices("campanha"));

            return countResponse.Count;
        }




        public async Task<bool?> ClearIndiceAsync(string index)
        {
            try
            {
                var deleteResponse = await _client.DeleteByQueryAsync(
                    new DeleteByQueryRequest(index)
                    {
                        Query = new MatchAllQuery()
                    });

                if (deleteResponse.IsValidResponse)
                {
                    _logger.LogInformation("Índice " + index + " limpo. " + deleteResponse.Deleted + " documentos removidos.", BaseLogType.LOG,  null);
                }
                    
                else
                {
                    _logger.LogError("Erro ao limpar índice: " + index, BaseLogType.LOG,  deleteResponse.DebugInformation);
                }

                return deleteResponse.IsSuccess();
                    
                
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao limpar índice antes da reindexação: ", BaseLogType.LOG, ex.Message);
                return false;
            }
        }




        public async Task<bool> IndexExistsAsync(string index)
        {
            var response = await _client.Indices.ExistsAsync(index);
            return response.Exists;
        }


        public async Task CreateIndexIfNotExistsAsync(string index)
        {
            var exists = await _client.Indices.ExistsAsync(index);
            if (exists.Exists) return;

            var response = await _client.Indices.CreateAsync(index, c => c
                .Settings(s => s
                    .Analysis(a => a
                        .Normalizers(n => n
                            .Custom("lowercase_normalizer", cn => cn
                                .Filter(new[] { "lowercase" })
                            )
                        )
                    )
                )
                .Mappings(m => m
                    .Properties<CampanhaDTO>(p => p
                        .Keyword(k => k.Guid)
                        .Text(t => t.Titulo)
                        .Text(t => t.Descricao)
                        .Keyword(k => k.StatusCampanha, kd => kd
                            .Normalizer("lowercase_normalizer"))  // ← normaliza para lowercase
                        .Keyword(k => k.DataInicio)
                        .Keyword(k => k.DataFim)
                        .FloatNumber(n => n.MetaFinanceira)
                        .FloatNumber(n => n.ValorArrecadado)
                    )
                )
            );
        }


        public async Task UpdateAsync(CampanhaDTO campanha)
        {
            var response = await _client.UpdateAsync<CampanhaDTO, CampanhaDTO>(
                "campanha",
                campanha.Guid.ToString(),
                u => u.Doc(campanha));

            if (!response.IsValidResponse)
                throw new Exception($"Falha ao atualizar no ElasticSearch: {response.DebugInformation}");
        }

    }
}
