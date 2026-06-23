using GearUp.Application.Clientes.Exceptions;
using GearUp.Application.Common.Exceptions;
using GearUp.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GearUp.Api.ErrorHandling;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, code, message) = exception switch
        {
            RegraNegocioException regra => (
                StatusCodes.Status422UnprocessableEntity,
                regra.Codigo,
                regra.Message),
            RecursoNaoEncontradoException recurso => (
                StatusCodes.Status404NotFound,
                recurso.Codigo,
                recurso.Message),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "NAO_AUTORIZADO",
                exception.Message),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "DADOS_INVALIDOS",
                exception.Message),
            ClienteDocumentoDuplicadoException => (
                StatusCodes.Status409Conflict,
                "RECURSO_DUPLICADO",
                exception.Message),
            ConflitoException conflito => (
                StatusCodes.Status409Conflict,
                conflito.Codigo,
                conflito.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "ERRO_INTERNO",
                "Ocorreu um erro inesperado.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Erro não tratado ao processar a requisição.");
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new ErrorResponse(code, message), cancellationToken);
        return true;
    }
}

internal sealed record ErrorResponse(string Code, string Message);
