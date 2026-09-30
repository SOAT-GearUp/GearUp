using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using GearUp.Api.IntegrationTests.Infrastructure;
using Microsoft.IdentityModel.Tokens;

namespace GearUp.Api.IntegrationTests.Autenticacao;

// Contrato entre a Lambda de autenticação por CPF (repositório
// gearup-lambda-auth, src/seguranca/token.mjs) e a API: o JWT emitido pela
// Lambda usa os nomes "crus" das claims (role, cliente_id, unique_name, amr)
// e precisa ser aceito pela API como um usuário do perfil Cliente.
public sealed class TokenLambdaCpfTests(GearUpApiFactory factory) : IntegrationTestBase(factory)
{
    private const string JwtKey = "gearup-integration-tests-jwt-key-change-me";

    private static string GerarTokenComoLambda(Guid clienteId, string nome = "Cliente Integração")
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, clienteId.ToString()),
            new Claim("role", "Cliente"),
            new Claim("cliente_id", clienteId.ToString()),
            new Claim("unique_name", nome),
            new Claim("amr", "cpf"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            "GearUp",
            "GearUp.Clients",
            claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private HttpClient CriarClienteComToken(string token)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task ListarOrdens_ComTokenDaLambda_DeveRetornarSomenteOrdensDoCliente()
    {
        var admin = await CreateAuthenticatedClientAsync();
        var clienteA = await CadastrarClienteAsync(admin);
        var clienteB = await CadastrarClienteAsync(admin);
        var ordemA = await CadastrarOrdemServicoAsync(admin, clienteA, await CadastrarVeiculoAsync(admin, clienteA));
        var ordemB = await CadastrarOrdemServicoAsync(admin, clienteB, await CadastrarVeiculoAsync(admin, clienteB));
        var cliente = CriarClienteComToken(GerarTokenComoLambda(clienteA));

        var response = await cliente.GetAsync("/api/ordens-servico");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains(ordemA.ToString(), corpo);
        Assert.DoesNotContain(ordemB.ToString(), corpo);
    }

    [Fact]
    public async Task ConsultarStatus_ComTokenDaLambda_DePropriaOrdem_DeveRetornar200()
    {
        var admin = await CreateAuthenticatedClientAsync();
        var clienteId = await CadastrarClienteAsync(admin);
        var ordemId = await CadastrarOrdemServicoAsync(admin, clienteId, await CadastrarVeiculoAsync(admin, clienteId));
        var cliente = CriarClienteComToken(GerarTokenComoLambda(clienteId));

        var response = await cliente.GetAsync($"/api/ordens-servico/{ordemId}/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListarClientes_ComTokenDaLambda_DeveRetornar403()
    {
        var cliente = CriarClienteComToken(GerarTokenComoLambda(Guid.NewGuid()));

        var response = await cliente.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ListarOrdens_ComTokenAssinadoComOutraChave_DeveRetornar401()
    {
        var claims = new[] { new Claim("role", "Cliente"), new Claim("cliente_id", Guid.NewGuid().ToString()) };
        var falso = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            "GearUp",
            "GearUp.Clients",
            claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes("uma-chave-diferente-com-mais-de-32-bytes")),
                SecurityAlgorithms.HmacSha256)));
        var cliente = CriarClienteComToken(falso);

        var response = await cliente.GetAsync("/api/ordens-servico");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
