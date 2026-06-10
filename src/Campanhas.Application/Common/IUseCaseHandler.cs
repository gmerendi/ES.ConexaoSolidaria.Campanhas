namespace Campanhas.Application.Common;

public interface IUseCaseHandler<in TCommand, TResult>
{
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
