using System.Net;
using GearUp.Api.IntegrationTests.Infrastructure;

namespace GearUp.Api.IntegrationTests.Clientes;

public sealed class ClientesEndpointsTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Listar_AutenticadoComoAtendente_DeveRetornar200()
    {
        var client = await CriarClienteAutenticadoAsync("atendente");

        var resposta = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }
}
