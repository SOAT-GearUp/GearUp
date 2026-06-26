using System.Net;
using System.Net.Http.Json;
using GearUp.Api.IntegrationTests.Infrastructure;
using GearUp.Domain.Enums;

namespace GearUp.Api.IntegrationTests.Estoque;

public sealed class EstoqueFluxoTests(GearUpApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Estoque_DeveCadastrarListarEMovimentarItem()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var itemId = await CadastrarItemEstoqueAsync(client, quantidadeInicial: 10);

        var listagem = await client.GetAsync("/api/estoque");
        listagem.EnsureSuccessStatusCode();

        var entrada = await client.PostAsJsonAsync($"/api/estoque/{itemId}/movimentacoes", new
        {
            tipo = TipoMovimentacaoEstoque.Entrada,
            quantidade = 5,
            motivo = "Reposição de teste"
        });

        Assert.Equal(HttpStatusCode.NoContent, entrada.StatusCode);

        var saida = await client.PostAsJsonAsync($"/api/estoque/{itemId}/movimentacoes", new
        {
            tipo = TipoMovimentacaoEstoque.Saida,
            quantidade = 3,
            motivo = "Baixa de teste"
        });

        Assert.Equal(HttpStatusCode.NoContent, saida.StatusCode);
    }

    [Fact]
    public async Task MovimentarSaida_ComEstoqueInsuficiente_DeveRetornarUnprocessableEntity()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var itemId = await CadastrarItemEstoqueAsync(client, quantidadeInicial: 1);

        var response = await client.PostAsJsonAsync($"/api/estoque/{itemId}/movimentacoes", new
        {
            tipo = TipoMovimentacaoEstoque.Saida,
            quantidade = 2,
            motivo = "Saída acima do saldo"
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
