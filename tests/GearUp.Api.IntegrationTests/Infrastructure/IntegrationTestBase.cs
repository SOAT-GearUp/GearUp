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

    /// <summary>
    /// Cria um cliente HTTP sem autenticação.
    /// </summary>
    protected HttpClient CriarClienteAnonimo() => Factory.CreateClient();

    /// <summary>
    /// Autentica um usuário existente e devolve um cliente HTTP com o header
    /// <c>Authorization: Bearer {token}</c> já preenchido.
    /// </summary>
    protected async Task<HttpClient> CriarClienteAutenticadoAsync(string usuario)
    {
        await Factory.GarantirUsuariosOperacionaisAsync();
        return await Factory.AutenticarAsync(usuario);
    }
}
