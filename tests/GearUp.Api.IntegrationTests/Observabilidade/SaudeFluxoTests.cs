using System.Net;
using GearUp.Api.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GearUp.Api.IntegrationTests.Observabilidade;

public sealed class SaudeFluxoTests(GearUpApiFactory factory) : IntegrationTestBase(factory)
{
    private readonly GearUpApiFactory factory = factory;

    [Fact]
    public async Task Liveness_SemAutenticacao_DeveRetornarHealthy()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Readiness_ComBancoDisponivel_DeveRetornarHealthy()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    // O kubelet chama as probes em HTTP puro na porta 8080. Este teste garante
    // que o UseHttpsRedirection não devolve 307 nesse cenário — um redirect
    // faria a probe falhar e o pod nunca ficaria Ready.
    [Fact]
    public async Task Probes_EmHttpSemTls_NaoDevemRedirecionar()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
            AllowAutoRedirect = false
        });

        var liveness = await client.GetAsync("/health/live");
        var readiness = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, liveness.StatusCode);
        Assert.Equal(HttpStatusCode.OK, readiness.StatusCode);
    }
}
