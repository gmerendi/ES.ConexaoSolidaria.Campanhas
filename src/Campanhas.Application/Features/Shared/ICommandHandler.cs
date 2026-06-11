namespace Campanhas.Application.Shared;

public interface IUseCaseHandler<in TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}