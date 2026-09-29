using System.Net;
using GearUp.Api.IntegrationTests.Infrastructure;

namespace GearUp.Api.IntegrationTests.Saude;

public sealed class HealthCheckTests(GearUpApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task HealthLive_QuandoApiDisponivel_DeveRetornarStatusEVersao()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await ReadJsonAsync(response);
        Assert.Equal("Healthy", json["status"]?.GetValue<string>());
        Assert.False(string.IsNullOrWhiteSpace(json["versao"]?.GetValue<string>()));
    }

    [Fact]
    public async Task HealthReady_QuandoBancoDisponivel_DeveRetornarStatusEVersao()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await ReadJsonAsync(response);
        Assert.Equal("Healthy", json["status"]?.GetValue<string>());
        Assert.Equal("1.0.0", json["versao"]?.GetValue<string>());

        var verificacoes = json["verificacoes"]?.AsArray();
        Assert.NotNull(verificacoes);
        Assert.Contains(verificacoes, item => item?["nome"]?.GetValue<string>() == "postgres");
    }

    [Fact]
    public async Task HealthLive_SemCorrelationId_DeveGerarHeaderNaResposta()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.True(response.Headers.TryGetValues("X-Correlation-ID", out var values));
        Assert.False(string.IsNullOrWhiteSpace(Assert.Single(values)));
    }

    [Fact]
    public async Task HealthLive_ComCorrelationIdValido_DevePropagarHeaderNaResposta()
    {
        const string correlationId = "teste-integracao-123";
        var client = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("X-Correlation-ID", correlationId);

        var response = await client.SendAsync(request);

        Assert.True(response.Headers.TryGetValues("X-Correlation-ID", out var values));
        Assert.Equal(correlationId, Assert.Single(values));
    }
}
