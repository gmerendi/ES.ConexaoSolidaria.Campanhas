using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Exceptions;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Resources;
using System.Net;
using System.Text.Json;

namespace Campanhas.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IBaseLogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, IBaseLogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdGenerator correlationIdGenerator)
    {
        var correlationId = correlationIdGenerator.Get() ?? "N/A";

        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogError(message: $"Erro de negócio detectado: " + ex.ErrorCode + " - " + ex.Message, BaseLogType.LOG,
                            data: ex,
                            correlationId: correlationId
                            );

            var statusCode = ExtrairStatusCode(ex.ErrorCode, HttpStatusCode.UnprocessableEntity);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var respostaElaborada = new
            {
                title = "A domain error occurred.",
                status = (int)statusCode,
                errors = new Dictionary<string, string[]>
                {
                    { "Domain", new[] { ex.Message } }
                },
                traceId = correlationId,
                code = ex.ErrorCode
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(respostaElaborada));
        }
        catch (BadHttpRequestException ex)
        {
            _logger.LogWarning(message: $"Falha na validação dos dados de entrada: " + ex.Message, BaseLogType.LOG,
                                data: ex,
                                correlationId: correlationId
                                );

            string errorCode = ex.Message;
            string mensagemTraduzida = ErrorMessages.GetString(errorCode);

            var statusCode = ExtrairStatusCode(errorCode, HttpStatusCode.BadRequest);

            await FormatarRespostaErroAsync(context, statusCode, errorCode, mensagemTraduzida);
        }
        catch (Exception ex)
        {
            _logger.LogError(message: $"Ocorreu um erro não tratado no servidor: " + ex.Message, BaseLogType.LOG,
                                data: ex,
                                correlationId: correlationId
                                );

            string codigoErroInesperado = "500_ERRO_INESPERADO";
            string mensagemInesperada = ErrorMessages.GetString(codigoErroInesperado);

            await FormatarRespostaErroAsync(context, HttpStatusCode.InternalServerError, codigoErroInesperado, mensagemInesperada);
        }
    }


    private static async Task FormatarRespostaErroAsync(HttpContext context, HttpStatusCode statusCode, string errorCode, string mensagem)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var resposta = new
        {
            code = errorCode,
            error = mensagem
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(resposta));
    }

    private static HttpStatusCode ExtrairStatusCode(string? errorCode, HttpStatusCode fallback)
    {
        if (!string.IsNullOrWhiteSpace(errorCode) && errorCode.Length >= 4)
        {
            var tresPrimeirosCaracteres = errorCode.Substring(0, 3);
            if (int.TryParse(tresPrimeirosCaracteres, out int code))
            {
                return (HttpStatusCode)code;
            }
        }
        return fallback;
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
