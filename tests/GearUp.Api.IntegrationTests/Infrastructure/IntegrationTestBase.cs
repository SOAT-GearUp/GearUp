using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace GearUp.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Classe base para os testes de integração. Compartilha uma única instância da
/// <see cref="CustomWebApplicationFactory"/> (e, portanto, do container PostgreSQL)
/// entre todos os testes da mesma classe via <see cref="IClassFixture{TFixture}"/>.
/// </summary>
public abstract class IntegrationTestBase(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    protected CustomWebApplicationFactory Factory { get; } = factory;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Cria um cliente HTTP sem autenticação.
    /// </summary>
    protected HttpClient CriarClienteAnonimo() => Factory.CreateClient();

    /// <summary>
    /// Autentica um dos usuários de seed (<c>atendente</c>, <c>auxiliar</c> ou
    /// <c>mecanico</c>) e devolve um cliente HTTP com o header
    /// <c>Authorization: Bearer {token}</c> já preenchido.
    /// </summary>
    protected async Task<HttpClient> CriarClienteAutenticadoAsync(string usuario)
    {
        var client = Factory.CreateClient();

        var resposta = await client.PostAsJsonAsync(
            "/api/autenticacao/login",
            new { Usuario = usuario, Senha = CustomWebApplicationFactory.SenhaSeed });

        resposta.EnsureSuccessStatusCode();

        var token = await resposta.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions)
            ?? throw new InvalidOperationException("Resposta de login sem corpo válido.");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);

        return client;
    }

    private sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiraEm);
}
