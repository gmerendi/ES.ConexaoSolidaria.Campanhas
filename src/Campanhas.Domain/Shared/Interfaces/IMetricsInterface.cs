namespace Campanhas.Domain.Shared.Interfaces;

public interface IMetricsService
{
    // ── Contadores de negócio ─────────────────────────────────────────────────
    void IncrementarIntencaoDoacao();
    void IncrementarCampanhaCriada();
    void IncrementarCampanhaConcluida();
    void IncrementarCampanhaCancelada();

    // ── Latência de requisições HTTP (para p90/p95/p99) ───────────────────────
    void RegistrarDuracaoRequisicao(string metodo, string rota, int statusCode, double duracaoSegundos);
}
