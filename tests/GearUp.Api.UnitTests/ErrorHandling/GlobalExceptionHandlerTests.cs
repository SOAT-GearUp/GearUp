using System.Text.Json;
using GearUp.Api.ErrorHandling;
using GearUp.Application.Common.Exceptions;
using GearUp.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace GearUp.Api.UnitTests.ErrorHandling;

public sealed class GlobalExceptionHandlerTests
{
    [Theory]
    [MemberData(nameof(ExcecoesMapeadas))]
    public async Task TryHandleAsync_DeveMapearExcecaoParaRespostaEsperada(
        Exception exception,
        int statusCode,
        string code,
        string message)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(statusCode, httpContext.Response.StatusCode);

        var response = await LerRespostaAsync(httpContext);
        Assert.Equal(code, response.Code);
        Assert.Equal(message, response.Message);
    }

    public static TheoryData<Exception, int, string, string> ExcecoesMapeadas()
    {
        return new TheoryData<Exception, int, string, string>
        {
            {
                new RegraNegocioException("REGRA_INVALIDA", "Regra violada."),
                StatusCodes.Status422UnprocessableEntity,
                "REGRA_INVALIDA",
                "Regra violada."
            },
            {
                new RecursoNaoEncontradoException("CLIENTE_NAO_ENCONTRADO", "Cliente nao encontrado."),
                StatusCodes.Status404NotFound,
                "CLIENTE_NAO_ENCONTRADO",
                "Cliente nao encontrado."
            },
            {
                new UnauthorizedAccessException("Sem acesso."),
                StatusCodes.Status401Unauthorized,
                "NAO_AUTORIZADO",
                "Sem acesso."
            },
            {
                new ConflitoException("PLACA_DUPLICADA", "Placa duplicada."),
                StatusCodes.Status409Conflict,
                "PLACA_DUPLICADA",
                "Placa duplicada."
            }
        };
    }

    [Fact]
    public async Task TryHandleAsync_ComExcecaoNaoMapeada_DeveRetornarErroInterno()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        await handler.TryHandleAsync(httpContext, new InvalidOperationException("Falha sensivel."), CancellationToken.None);

        var response = await LerRespostaAsync(httpContext);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        Assert.Equal("ERRO_INTERNO", response.Code);
        Assert.Equal("Ocorreu um erro inesperado.", response.Message);
    }

    private static async Task<(string Code, string Message)> LerRespostaAsync(HttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;

        using var document = await JsonDocument.ParseAsync(httpContext.Response.Body);
        var root = document.RootElement;

        return (
            root.GetProperty("code").GetString()!,
            root.GetProperty("message").GetString()!);
    }
}
