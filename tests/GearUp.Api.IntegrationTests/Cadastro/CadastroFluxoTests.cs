using System.Net;
using System.Net.Http.Json;
using GearUp.Api.IntegrationTests.Infrastructure;

namespace GearUp.Api.IntegrationTests.Cadastro;

public sealed class CadastroFluxoTests(GearUpApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task ClienteEVeiculo_DeveExecutarCadastroConsultaAtualizacaoEExclusaoLogica()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var clienteId = await CadastrarClienteAsync(client);
        var veiculoId = await CadastrarVeiculoAsync(client, clienteId);

        var consultaCliente = await client.GetAsync($"/api/clientes/{clienteId}");
        consultaCliente.EnsureSuccessStatusCode();

        var atualizacaoCliente = await client.PutAsJsonAsync($"/api/clientes/{clienteId}", new
        {
            nome = "Cliente Integração Atualizado",
            email = $"cliente.atualizado.{Guid.NewGuid():N}@gearup.test",
            telefone = "11988887777"
        });

        Assert.Equal(HttpStatusCode.NoContent, atualizacaoCliente.StatusCode);

        var atualizacaoVeiculo = await client.PutAsJsonAsync($"/api/clientes/{clienteId}/veiculos/{veiculoId}", new
        {
            placa = NovaPlaca(),
            marca = "Toyota",
            modelo = "Corolla",
            ano = 2023
        });

        Assert.Equal(HttpStatusCode.NoContent, atualizacaoVeiculo.StatusCode);

        var exclusao = await client.DeleteAsync($"/api/clientes/{clienteId}");
        Assert.Equal(HttpStatusCode.NoContent, exclusao.StatusCode);

        var consultaExcluido = await client.GetAsync($"/api/clientes/{clienteId}");
        Assert.Equal(HttpStatusCode.NotFound, consultaExcluido.StatusCode);
    }

    [Fact]
    public async Task CadastrarCliente_ComDocumentoDuplicado_DeveRetornarConflict()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var documento = NovoCpf();

        await CadastrarClienteAsync(client, documento);

        var response = await client.PostAsJsonAsync("/api/clientes", new
        {
            nome = "Cliente Documento Duplicado",
            documento,
            email = $"duplicado.{Guid.NewGuid():N}@gearup.test",
            telefone = "11999997777"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
