using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using GearUp.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GearUp.Api.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase(GearUpApiFactory factory) : IClassFixture<GearUpApiFactory>
{
    private static int cpfSequence;
    private static int placaSequence;

    protected HttpClient CreateClient()
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    protected async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = CreateClient();
        var token = await LoginAsync(client, GearUpApiFactory.AdminUser, GearUpApiFactory.AdminPassword);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected static async Task<string> LoginAsync(HttpClient client, string usuario, string senha)
    {
        var response = await client.PostAsJsonAsync("/api/autenticacao/login", new
        {
            usuario,
            senha
        });

        response.EnsureSuccessStatusCode();

        var json = await ReadJsonAsync(response);
        return json["accessToken"]?.GetValue<string>()
            ?? throw new InvalidOperationException("Token não retornado pela API.");
    }

    protected static async Task<JsonNode> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        return JsonNode.Parse(content)
            ?? throw new InvalidOperationException("Resposta JSON inválida.");
    }

    protected static async Task<Guid> ReadGuidAsync(HttpResponseMessage response, string propertyName = "id")
    {
        var json = await ReadJsonAsync(response);
        var value = json[propertyName]?.GetValue<string>();

        return Guid.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException($"A propriedade {propertyName} não contém um Guid válido.");
    }

    protected async Task<Guid> CadastrarClienteAsync(HttpClient client, string? documento = null)
    {
        var response = await client.PostAsJsonAsync("/api/clientes", new
        {
            nome = "Cliente Integração",
            documento = documento ?? NovoCpf(),
            email = $"cliente.{Guid.NewGuid():N}@gearup.test",
            telefone = "11999998888"
        });

        response.EnsureSuccessStatusCode();
        return await ReadGuidAsync(response);
    }

    protected async Task<Guid> CadastrarVeiculoAsync(HttpClient client, Guid clienteId, string? placa = null)
    {
        var response = await client.PostAsJsonAsync($"/api/clientes/{clienteId}/veiculos", new
        {
            placa = placa ?? NovaPlaca(),
            marca = "Honda",
            modelo = "Civic",
            ano = 2022
        });

        response.EnsureSuccessStatusCode();
        return await ReadGuidAsync(response, "veiculoId");
    }

    protected async Task<Guid> CadastrarItemEstoqueAsync(HttpClient client, decimal quantidadeInicial = 10)
    {
        var response = await client.PostAsJsonAsync("/api/estoque", new
        {
            nome = $"Filtro de óleo {Guid.NewGuid():N}",
            tipo = TipoItemEstoque.Peca,
            precoUnitario = 49.90m,
            quantidadeInicial
        });

        response.EnsureSuccessStatusCode();
        return await ReadGuidAsync(response);
    }

    protected async Task<Guid> CadastrarOrdemServicoAsync(HttpClient client, Guid clienteId, Guid veiculoId)
    {
        var response = await client.PostAsJsonAsync("/api/ordens-servico", new
        {
            clienteId,
            veiculoId,
            solicitacaoInicial = "Troca de óleo e revisão preventiva",
            prioridade = PrioridadeOrdemServico.Normal,
            prazo = DateTimeOffset.UtcNow.AddDays(3)
        });

        response.EnsureSuccessStatusCode();
        return await ReadGuidAsync(response);
    }

    protected async Task<Guid> CriarUsuarioClienteAsync(HttpClient client, Guid clienteId, string usuario, string senha)
    {
        var response = await client.PostAsJsonAsync("/api/usuarios", new
        {
            usuario,
            senha,
            perfil = PerfilUsuario.Cliente,
            clienteId
        });

        response.EnsureSuccessStatusCode();
        return await ReadGuidAsync(response);
    }

    protected static string NovoCpf()
    {
        var numeroBase = (100000000 + Interlocked.Increment(ref cpfSequence)).ToString("D9");
        var primeiroDigito = CalcularDigito(numeroBase, 10);
        var segundoDigito = CalcularDigito(numeroBase + primeiroDigito, 11);

        return numeroBase + primeiroDigito + segundoDigito;
    }

    protected static string NovaPlaca()
    {
        var sequencia = Interlocked.Increment(ref placaSequence) % 100;
        return $"TST1A{sequencia:00}";
    }

    private static int CalcularDigito(string numeros, int pesoInicial)
    {
        var soma = 0;

        for (var indice = 0; indice < numeros.Length; indice++)
            soma += (numeros[indice] - '0') * (pesoInicial - indice);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
