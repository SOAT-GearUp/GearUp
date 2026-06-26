using System.Net;
using System.Net.Http.Json;
using GearUp.Api.IntegrationTests.Infrastructure;

namespace GearUp.Api.IntegrationTests.Autenticacao;

public sealed class AutenticacaoFluxoTests(GearUpApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Login_ComAdminSeedado_DeveRetornarToken()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/autenticacao/login", new
        {
            usuario = GearUpApiFactory.AdminUser,
            senha = GearUpApiFactory.AdminPassword
        });

        response.EnsureSuccessStatusCode();

        var json = await ReadJsonAsync(response);
        Assert.False(string.IsNullOrWhiteSpace(json["accessToken"]?.GetValue<string>()));
    }

    [Fact]
    public async Task EndpointProtegido_SemToken_DeveRetornarUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
