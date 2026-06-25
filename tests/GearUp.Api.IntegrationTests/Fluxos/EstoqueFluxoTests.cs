using System.Net;
using System.Net.Http.Json;
using GearUp.Api.Contracts.Estoque;
using GearUp.Api.IntegrationTests.Infrastructure;
using GearUp.Application.Estoque.Cadastrar;
using GearUp.Application.Estoque.Listar;
using GearUp.Domain.Enums;

namespace GearUp.Api.IntegrationTests.Fluxos;

/// <summary>
/// Testes de integração do fluxo de Estoque, exercitando o
/// <c>EstoqueController</c> (rota <c>api/estoque</c>, papéis Atendente/Auxiliar)
/// de ponta a ponta via HTTP: cadastro, listagem, movimentação e regras de saldo.
/// </summary>
public sealed class EstoqueFluxoTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CadastrarItem_ComoAtendente_DeveCriarEAparecerNaListagemComSaldoInicial()
    {
        var client = await CriarClienteAutenticadoAsync("atendente");

        var nome = $"Filtro de óleo {Guid.NewGuid():N}";
        var id = await CadastrarItemAsync(
            client,
            new CriarItemEstoqueRequest(nome, TipoItemEstoque.Peca, PrecoUnitario: 49.90m, QuantidadeInicial: 10));

        var item = await ObterItemAsync(client, id);

        Assert.NotNull(item);
        Assert.Equal(nome, item!.Nome);
        Assert.Equal(49.90m, item.PrecoUnitario);
        Assert.Equal(10m, item.QuantidadeDisponivel);
    }

    [Fact]
    public async Task Movimentar_EntradaESaida_DeveAtualizarSaldoDisponivel()
    {
        var client = await CriarClienteAutenticadoAsync("atendente");

        var id = await CadastrarItemAsync(
            client,
            new CriarItemEstoqueRequest($"Óleo 5W30 {Guid.NewGuid():N}", TipoItemEstoque.Insumo, PrecoUnitario: 35m));

        var entrada = await client.PostAsJsonAsync(
            $"/api/estoque/{id}/movimentacoes",
            new MovimentarEstoqueRequest(TipoMovimentacaoEstoque.Entrada, Quantidade: 10, Motivo: "Compra de fornecedor"));
        Assert.Equal(HttpStatusCode.NoContent, entrada.StatusCode);

        var saida = await client.PostAsJsonAsync(
            $"/api/estoque/{id}/movimentacoes",
            new MovimentarEstoqueRequest(TipoMovimentacaoEstoque.Saida, Quantidade: 4, Motivo: "Uso em ordem de serviço"));
        Assert.Equal(HttpStatusCode.NoContent, saida.StatusCode);

        var item = await ObterItemAsync(client, id);
        Assert.NotNull(item);
        Assert.Equal(6m, item!.QuantidadeDisponivel);
    }

    [Fact]
    public async Task Movimentar_SaidaMaiorQueSaldo_DeveRetornar422()
    {
        var client = await CriarClienteAutenticadoAsync("atendente");

        var id = await CadastrarItemAsync(
            client,
            new CriarItemEstoqueRequest($"Pastilha de freio {Guid.NewGuid():N}", TipoItemEstoque.Peca, PrecoUnitario: 80m, QuantidadeInicial: 5));

        var resposta = await client.PostAsJsonAsync(
            $"/api/estoque/{id}/movimentacoes",
            new MovimentarEstoqueRequest(TipoMovimentacaoEstoque.Saida, Quantidade: 10, Motivo: "Saída acima do saldo"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);

        // O saldo deve permanecer intacto após a regra de negócio rejeitar a saída.
        var item = await ObterItemAsync(client, id);
        Assert.NotNull(item);
        Assert.Equal(5m, item!.QuantidadeDisponivel);
    }

    private static async Task<Guid> CadastrarItemAsync(HttpClient client, CriarItemEstoqueRequest request)
    {
        var resposta = await client.PostAsJsonAsync("/api/estoque", request);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var criado = await resposta.Content.ReadFromJsonAsync<CadastrarEstoqueItemResult>();
        Assert.NotNull(criado);
        Assert.NotEqual(Guid.Empty, criado!.Id);

        return criado.Id;
    }

    private static async Task<ListarEstoqueItemResult?> ObterItemAsync(HttpClient client, Guid id)
    {
        var resposta = await client.GetAsync("/api/estoque");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var itens = await resposta.Content.ReadFromJsonAsync<List<ListarEstoqueItemResult>>();
        Assert.NotNull(itens);

        return itens!.SingleOrDefault(i => i.Id == id);
    }
}
