namespace Campanhas.Domain.Shared.Interfaces
{
    public interface IElasticSearchSeedService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
