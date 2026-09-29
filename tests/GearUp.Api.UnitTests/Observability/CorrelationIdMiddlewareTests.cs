using GearUp.Api.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace GearUp.Api.UnitTests.Observability;

public sealed class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_SemCorrelationId_DeveGerarHeaderNaResposta()
    {
        var context = CriarHttpContext();
        var middleware = CriarMiddleware();

        await middleware.InvokeAsync(context);

        var correlationId = context.Response.Headers["X-Correlation-ID"].ToString();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
    }

    [Fact]
    public async Task InvokeAsync_ComCorrelationIdValido_DevePropagarHeaderNaResposta()
    {
        const string correlationId = "teste-unitario-123";
        var context = CriarHttpContext();
        context.Request.Headers["X-Correlation-ID"] = correlationId;
        var middleware = CriarMiddleware();

        await middleware.InvokeAsync(context);

        Assert.Equal(correlationId, context.Response.Headers["X-Correlation-ID"].ToString());
    }

    private static CorrelationIdMiddleware CriarMiddleware() =>
        new(
            async context => await context.Response.WriteAsync("ok"),
            NullLogger<CorrelationIdMiddleware>.Instance);

    private static DefaultHttpContext CriarHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }
}
