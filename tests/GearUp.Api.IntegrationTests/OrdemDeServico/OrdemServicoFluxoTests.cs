using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using GearUp.Api.IntegrationTests.Infrastructure;
using GearUp.Domain.Enums;

namespace GearUp.Api.IntegrationTests.OrdemDeServico;

public sealed class OrdemServicoFluxoTests(GearUpApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task OrdemServico_DeveExecutarDiagnosticoOrcamentoAprovacaoExecucaoEEntrega()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var clienteId = await CadastrarClienteAsync(client);
        var veiculoId = await CadastrarVeiculoAsync(client, clienteId);
        var ordemServicoId = await CadastrarOrdemServicoAsync(client, clienteId, veiculoId);

        var iniciarDiagnostico = await client.PostAsync($"/api/ordens-servico/{ordemServicoId}/diagnostico/iniciar", null);
        Assert.Equal(HttpStatusCode.NoContent, iniciarDiagnostico.StatusCode);

        var registrarDiagnostico = await client.PostAsJsonAsync($"/api/ordens-servico/{ordemServicoId}/diagnostico", new
        {
            descricao = "Óleo vencido e filtro saturado."
        });
        Assert.Equal(HttpStatusCode.NoContent, registrarDiagnostico.StatusCode);

        var criarOrcamento = await client.PostAsJsonAsync($"/api/ordens-servico/{ordemServicoId}/orcamentos", new
        {
            itens = new[]
            {
                new
                {
                    tipo = TipoItemOrcamento.Servico,
                    descricao = "Troca de óleo",
                    quantidade = 1,
                    valorUnitario = 120m,
                    estoqueItemId = (Guid?)null
                }
            }
        });
        criarOrcamento.EnsureSuccessStatusCode();
        var orcamentoId = await ReadGuidAsync(criarOrcamento);

        var adicionarItem = await client.PostAsJsonAsync($"/api/ordens-servico/{ordemServicoId}/orcamentos/{orcamentoId}/itens", new
        {
            tipo = TipoItemOrcamento.MaoDeObra,
            descricao = "Mão de obra complementar",
            quantidade = 1,
            valorUnitario = 80m,
            estoqueItemId = (Guid?)null
        });
        Assert.Equal(HttpStatusCode.NoContent, adicionarItem.StatusCode);

        var itensOrcamento = await ObterItensOrcamentoAsync(client, ordemServicoId, orcamentoId);
        var itemId = itensOrcamento[0];
        var itemParaRemoverId = itensOrcamento[1];

        var atualizarItem = await client.PutAsJsonAsync($"/api/ordens-servico/{ordemServicoId}/orcamentos/{orcamentoId}/itens/{itemId}", new
        {
            tipo = TipoItemOrcamento.Servico,
            descricao = "Troca de óleo revisada",
            quantidade = 1,
            valorUnitario = 130m,
            estoqueItemId = (Guid?)null
        });
        Assert.Equal(HttpStatusCode.NoContent, atualizarItem.StatusCode);

        var removerItem = await client.DeleteAsync($"/api/ordens-servico/{ordemServicoId}/orcamentos/{orcamentoId}/itens/{itemParaRemoverId}");
        Assert.Equal(HttpStatusCode.NoContent, removerItem.StatusCode);

        var decidir = await client.PostAsJsonAsync($"/api/ordens-servico/{ordemServicoId}/orcamentos/{orcamentoId}/decisao", new
        {
            aprovado = true
        });
        Assert.Equal(HttpStatusCode.NoContent, decidir.StatusCode);

        var aguardarExecucao = await client.PatchAsJsonAsync($"/api/ordens-servico/{ordemServicoId}/status", new
        {
            status = StatusOrdemServico.AguardandoExecucao
        });
        Assert.Equal(HttpStatusCode.NoContent, aguardarExecucao.StatusCode);

        var iniciarExecucao = await client.PatchAsJsonAsync($"/api/ordens-servico/{ordemServicoId}/status", new
        {
            status = StatusOrdemServico.EmExecucao
        });
        Assert.Equal(HttpStatusCode.NoContent, iniciarExecucao.StatusCode);

        var finalizar = await client.PatchAsJsonAsync($"/api/ordens-servico/{ordemServicoId}/status", new
        {
            status = StatusOrdemServico.Finalizada
        });
        Assert.Equal(HttpStatusCode.NoContent, finalizar.StatusCode);

        var entregar = await client.PatchAsJsonAsync($"/api/ordens-servico/{ordemServicoId}/status", new
        {
            status = StatusOrdemServico.Entregue
        });
        Assert.Equal(HttpStatusCode.NoContent, entregar.StatusCode);

        var consulta = await client.GetAsync($"/api/ordens-servico/{ordemServicoId}");
        consulta.EnsureSuccessStatusCode();

        var os = await ReadJsonAsync(consulta);
        Assert.Equal((int)StatusOrdemServico.Entregue, os["status"]?.GetValue<int>());
    }

    [Fact]
    public async Task AlterarStatus_ComTransicaoInvalida_DeveRetornarUnprocessableEntity()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var clienteId = await CadastrarClienteAsync(client);
        var veiculoId = await CadastrarVeiculoAsync(client, clienteId);
        var ordemServicoId = await CadastrarOrdemServicoAsync(client, clienteId, veiculoId);

        var response = await client.PatchAsJsonAsync($"/api/ordens-servico/{ordemServicoId}/status", new
        {
            status = StatusOrdemServico.Finalizada
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Metricas_DeveRetornarTempoMedioExecucao()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/ordens-servico/metricas/tempo-medio-execucao");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<IReadOnlyList<Guid>> ObterItensOrcamentoAsync(HttpClient client, Guid ordemServicoId, Guid orcamentoId)
    {
        var response = await client.GetAsync($"/api/ordens-servico/{ordemServicoId}");
        response.EnsureSuccessStatusCode();

        var json = await ReadJsonAsync(response);
        var orcamentos = json["orcamentos"]?.AsArray()
            ?? throw new InvalidOperationException("Orçamentos não retornados na consulta da OS.");

        var orcamento = orcamentos
            .OfType<JsonObject>()
            .FirstOrDefault(o => Guid.Parse(o["id"]!.GetValue<string>()) == orcamentoId)
            ?? throw new InvalidOperationException("Orçamento criado não encontrado na consulta da OS.");

        var itens = orcamento["itens"]?.AsArray()
            ?? throw new InvalidOperationException("Itens do orçamento não retornados na consulta da OS.");

        return itens
            .Select(item => Guid.Parse(item!["id"]!.GetValue<string>()))
            .ToList();
    }
}
