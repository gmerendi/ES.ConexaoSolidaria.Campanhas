namespace Campanhas.Application.Common;

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public ResultErrorType ErrorType { get; }

    private Result(bool isSuccess, T? data, string? errorMessage, ResultErrorType errorType)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public static Result<T> Success(T data) =>
        new(true, data, null, ResultErrorType.None);

    public static Result<T> Failure(string errorMessage, ResultErrorType errorType = ResultErrorType.DomainError) =>
        new(false, default, errorMessage, errorType);

    public static Result<T> NotFound(string errorMessage) =>
        new(false, default, errorMessage, ResultErrorType.NotFound);
}
