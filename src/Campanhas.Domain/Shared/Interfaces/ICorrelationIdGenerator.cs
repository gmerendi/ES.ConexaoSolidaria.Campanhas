namespace Campanhas.Domain.Shared.Interfaces
{
    public interface ICorrelationIdGenerator
    {
        string Get();
        void Set(string correlationId);
    }
}
