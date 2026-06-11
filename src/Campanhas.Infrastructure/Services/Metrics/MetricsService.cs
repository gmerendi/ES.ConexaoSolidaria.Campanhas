using Campanhas.Domain.Shared.Interfaces;
using Prometheus;

namespace Camapanhas.Infrastructure.Services.Metrics;

/// <summary>
/// Implementação do serviço de métricas usando prometheus-net.
/// Registra contadores de negócio e histogramas de latência HTTP.
/// As métricas são expostas automaticamente em /metrics pelo middleware do Prometheus.
/// </summary>
public class MetricsService : IMetricsService
{
    // ── Contadores de negócio ─────────────────────────────────────────────────

    private static readonly Counter _intencaoDoacaoCounter = Prometheus.Metrics.CreateCounter(
        "campanha_intencoes_total",
        "Número total de intencoes de doacao realizados.");

    private static readonly Counter _campanhaCriadaCounter = Prometheus.Metrics.CreateCounter(
        "campanhas_criadas_total",
        "Número total de novas campanhas cadastrados.");

    private static readonly Counter _campanhaConcluidaCounter = Prometheus.Metrics.CreateCounter(
        "campahas_concluidas_total",
        "Número total de campanhas concluidas.");

    private static readonly Counter _campanhaCanceladaCounter = Prometheus.Metrics.CreateCounter(
        "campahas_canceladas_total",
        "Número total de campanhas canceladas.");

    // ── Histograma de latência HTTP (alimenta p90/p95/p99) ────────────────────

    private static readonly Histogram _duracaoRequisicao = Prometheus.Metrics.CreateHistogram(
        "http_request_duration_seconds",
        "Duração das requisições HTTP em segundos.",
        new HistogramConfiguration
        {
            // Buckets calibrados para APIs REST típicas (ms até segundos)
            Buckets = new[] { 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1.0, 2.5, 5.0 },
            LabelNames = new[] { "method", "route", "status_code" }
        });

    // ── Implementação da interface ────────────────────────────────────────────

    public void IncrementarIntencaoDoacao() => _intencaoDoacaoCounter.Inc();
    public void IncrementarCampanhaCriada() => _campanhaCriadaCounter.Inc();
    public void IncrementarCampanhaConcluida() => _campanhaConcluidaCounter.Inc();
    public void IncrementarCampanhaCancelada() => _campanhaCanceladaCounter.Inc();
    public void RegistrarDuracaoRequisicao(string metodo, string rota, int statusCode, double duracaoSegundos)
        => _duracaoRequisicao
               .WithLabels(metodo, rota, statusCode.ToString())
               .Observe(duracaoSegundos);
}
