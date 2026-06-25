using System.Net;
using System.Net.Http.Json;
using GearUp.Api.IntegrationTests.Infrastructure;
using GearUp.Domain.Enums;

namespace GearUp.Api.IntegrationTests.Fluxos;

/// <summary>
/// Testes de integração de ponta a ponta (via HTTP) do fluxo do bounded context
/// <c>Diagnóstico &amp; Orçamento</c>: recepção da OS, diagnóstico, criação do
/// orçamento, edição de itens e decisão (aprovação/recusa). Valida tanto os
/// status codes quanto a evolução do estado da OS e do orçamento.
/// </summary>
public sealed class DiagnosticoOrcamentoFluxoTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task FluxoCompleto_DiagnosticarOrcarEAprovar_DeveEvoluirOsAteAguardandoPecasInsumos()
    {
        var atendente = await CriarClienteAutenticadoAsync("atendente");
        var mecanico = await CriarClienteAutenticadoAsync("mecanico");

        // 1. Setup: cliente + veículo + OS.
        var (_, _, ordemServicoId) = await CriarOrdemServicoAsync(atendente);

        var osRecebida = await ObterOrdemServicoAsync(atendente, ordemServicoId);
        Assert.Equal(StatusOrdemServico.Recebida, osRecebida.Status);
        Assert.Empty(osRecebida.Orcamentos);

        // 2. Iniciar diagnóstico (Mecanico): Recebida -> EmDiagnostico.
        var iniciar = await mecanico.PostAsync(
            $"/api/ordens-servico/{ordemServicoId}/diagnostico/iniciar", null);
        Assert.Equal(HttpStatusCode.NoContent, iniciar.StatusCode);
        Assert.Equal(
            StatusOrdemServico.EmDiagnostico,
            (await ObterOrdemServicoAsync(atendente, ordemServicoId)).Status);

        // 2b. Registrar diagnóstico (Mecanico): EmDiagnostico -> AguardandoOrcamento.
        var diagnosticar = await mecanico.PostAsJsonAsync(
            $"/api/ordens-servico/{ordemServicoId}/diagnostico",
            new { Descricao = "Pastilhas de freio gastas e disco empenado." });
        Assert.Equal(HttpStatusCode.NoContent, diagnosticar.StatusCode);
        Assert.Equal(
            StatusOrdemServico.AguardandoOrcamento,
            (await ObterOrdemServicoAsync(atendente, ordemServicoId)).Status);

        // 3. Criar orçamento (Atendente): OS -> AguardandoAprovacao.
        var orcamentoId = await CriarOrcamentoAsync(atendente, ordemServicoId);

        var osComOrcamento = await ObterOrdemServicoAsync(atendente, ordemServicoId);
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, osComOrcamento.Status);
        var orcamento = Assert.Single(osComOrcamento.Orcamentos);
        Assert.Equal(StatusOrcamento.Pendente, orcamento.Status);
        Assert.Single(orcamento.Itens);

        // 4. Adicionar item ao orçamento (Atendente): orçamento passa a ter 2 itens.
        var adicionar = await atendente.PostAsJsonAsync(
            $"/api/ordens-servico/{ordemServicoId}/orcamentos/{orcamentoId}/itens",
            new
            {
                Tipo = TipoItemOrcamento.Peca,
                Descricao = "Jogo de pastilhas de freio",
                Quantidade = 1m,
                ValorUnitario = 180m,
                EstoqueItemId = (Guid?)null,
            });
        Assert.Equal(HttpStatusCode.NoContent, adicionar.StatusCode);

        var osComItemAdicional = await ObterOrdemServicoAsync(atendente, ordemServicoId);
        var orcamentoComDoisItens = Assert.Single(osComItemAdicional.Orcamentos);
        Assert.Equal(2, orcamentoComDoisItens.Itens.Count);
        // 1 item de mão de obra (150) + 1 peça (180).
        Assert.Equal(330m, orcamentoComDoisItens.ValorTotal);

        // 5. Decidir: aprovar (Atendente): OS -> AguardandoPecasInsumos.
        var decidir = await atendente.PostAsJsonAsync(
            $"/api/ordens-servico/{ordemServicoId}/orcamentos/{orcamentoId}/decisao",
            new { Aprovado = true });
        Assert.Equal(HttpStatusCode.NoContent, decidir.StatusCode);

        var osDecidida = await ObterOrdemServicoAsync(atendente, ordemServicoId);
        Assert.Equal(StatusOrdemServico.AguardandoPecasInsumos, osDecidida.Status);
        Assert.Equal(StatusOrcamento.Aprovado, Assert.Single(osDecidida.Orcamentos).Status);
    }

    [Fact]
    public async Task FluxoCompleto_RecusarOrcamento_DeveVoltarOsParaAguardandoOrcamento()
    {
        var atendente = await CriarClienteAutenticadoAsync("atendente");
        var mecanico = await CriarClienteAutenticadoAsync("mecanico");

        var (_, _, ordemServicoId) = await CriarOrdemServicoAsync(atendente);
        await mecanico.PostAsync($"/api/ordens-servico/{ordemServicoId}/diagnostico/iniciar", null);
        await mecanico.PostAsJsonAsync(
            $"/api/ordens-servico/{ordemServicoId}/diagnostico",
            new { Descricao = "Necessária troca de embreagem." });
        var orcamentoId = await CriarOrcamentoAsync(atendente, ordemServicoId);

        var recusa = await atendente.PostAsJsonAsync(
            $"/api/ordens-servico/{ordemServicoId}/orcamentos/{orcamentoId}/decisao",
            new { Aprovado = false });
        Assert.Equal(HttpStatusCode.NoContent, recusa.StatusCode);

        var os = await ObterOrdemServicoAsync(atendente, ordemServicoId);
        Assert.Equal(StatusOrdemServico.AguardandoOrcamento, os.Status);
        Assert.Equal(StatusOrcamento.Rejeitado, Assert.Single(os.Orcamentos).Status);
    }

    [Fact]
    public async Task IniciarDiagnostico_ComoAtendente_DeveRetornar403()
    {
        var atendente = await CriarClienteAutenticadoAsync("atendente");
        var (_, _, ordemServicoId) = await CriarOrdemServicoAsync(atendente);

        var resposta = await atendente.PostAsync(
            $"/api/ordens-servico/{ordemServicoId}/diagnostico/iniciar", null);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task RegistrarDiagnostico_OsRecemRecebida_DeveRetornar422()
    {
        var atendente = await CriarClienteAutenticadoAsync("atendente");
        var mecanico = await CriarClienteAutenticadoAsync("mecanico");
        var (_, _, ordemServicoId) = await CriarOrdemServicoAsync(atendente);

        // OS ainda em "Recebida": registrar diagnóstico exige "EmDiagnostico".
        var resposta = await mecanico.PostAsJsonAsync(
            $"/api/ordens-servico/{ordemServicoId}/diagnostico",
            new { Descricao = "Tentativa fora de ordem." });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    // ---- Helpers ----------------------------------------------------------

    /// <summary>
    /// Cria cliente, veículo e a ordem de serviço associada (todos via Atendente),
    /// retornando os identificadores gerados. A OS resultante fica em <c>Recebida</c>.
    /// </summary>
    private static async Task<(Guid ClienteId, Guid VeiculoId, Guid OrdemServicoId)> CriarOrdemServicoAsync(
        HttpClient atendente)
    {
        var sufixo = Guid.NewGuid().ToString("N")[..8];

        var respostaCliente = await atendente.PostAsJsonAsync(
            "/api/clientes",
            new
            {
                Nome = $"Cliente Fluxo Orçamento {sufixo}",
                Documento = GerarCpf(),
                Email = $"fluxo.orcamento.{sufixo}@teste.com",
                Telefone = "11999990000",
            });
        Assert.Equal(HttpStatusCode.Created, respostaCliente.StatusCode);
        var clienteId = (await LerAsync<IdResponse>(respostaCliente)).Id;

        var respostaVeiculo = await atendente.PostAsJsonAsync(
            $"/api/clientes/{clienteId}/veiculos",
            new
            {
                Placa = GerarPlaca(),
                Marca = "Volkswagen",
                Modelo = "Golf",
                Ano = 2020,
            });
        Assert.Equal(HttpStatusCode.Created, respostaVeiculo.StatusCode);
        var veiculoId = (await LerAsync<VeiculoResponse>(respostaVeiculo)).VeiculoId;

        var respostaOs = await atendente.PostAsJsonAsync(
            "/api/ordens-servico",
            new
            {
                ClienteId = clienteId,
                VeiculoId = veiculoId,
                SolicitacaoInicial = "Barulho ao frear.",
                Prioridade = PrioridadeOrdemServico.Normal,
                Prazo = (DateTimeOffset?)null,
            });
        Assert.Equal(HttpStatusCode.Created, respostaOs.StatusCode);
        var ordemServicoId = (await LerAsync<IdResponse>(respostaOs)).Id;

        return (clienteId, veiculoId, ordemServicoId);
    }

    /// <summary>
    /// Cria um orçamento com um item de mão de obra (R$ 150) para a OS informada
    /// e retorna o identificador do orçamento. Pressupõe a OS em
    /// <c>AguardandoOrcamento</c>.
    /// </summary>
    private static async Task<Guid> CriarOrcamentoAsync(HttpClient atendente, Guid ordemServicoId)
    {
        var resposta = await atendente.PostAsJsonAsync(
            $"/api/ordens-servico/{ordemServicoId}/orcamentos",
            new
            {
                Itens = new[]
                {
                    new
                    {
                        Tipo = TipoItemOrcamento.MaoDeObra,
                        Descricao = "Troca de pastilhas e retífica de disco",
                        Quantidade = 1m,
                        ValorUnitario = 150m,
                        EstoqueItemId = (Guid?)null,
                    },
                },
            });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await LerAsync<OrcamentoCriadoResponse>(resposta)).Id;
    }

    private static async Task<OrdemServicoResponse> ObterOrdemServicoAsync(HttpClient client, Guid ordemServicoId)
    {
        var resposta = await client.GetAsync($"/api/ordens-servico/{ordemServicoId}");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return await LerAsync<OrdemServicoResponse>(resposta);
    }

    private static async Task<T> LerAsync<T>(HttpResponseMessage resposta)
        => await resposta.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException($"Resposta sem corpo para {typeof(T).Name}.");

    private sealed record IdResponse(Guid Id);

    private sealed record VeiculoResponse(Guid VeiculoId);

    private sealed record OrcamentoCriadoResponse(Guid Id, int Versao, decimal ValorTotal);

    private sealed record OrdemServicoResponse(
        Guid Id,
        Guid ClienteId,
        StatusOrdemServico Status,
        IReadOnlyList<OrcamentoResponse> Orcamentos);

    private sealed record OrcamentoResponse(
        Guid Id,
        StatusOrcamento Status,
        decimal ValorTotal,
        IReadOnlyList<ItemResponse> Itens);

    private sealed record ItemResponse(Guid Id, string Descricao, decimal Quantidade, decimal ValorUnitario);

    private static string GerarCpf()
    {
        var random = new Random();
        var digitos = new int[11];
        for (var i = 0; i < 9; i++)
            digitos[i] = random.Next(0, 10);

        digitos[9] = CalcularDigitoVerificador(digitos, 9, 10);
        digitos[10] = CalcularDigitoVerificador(digitos, 10, 11);

        return string.Concat(digitos);
    }

    private static int CalcularDigitoVerificador(int[] digitos, int quantidade, int pesoInicial)
    {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
            soma += digitos[i] * (pesoInicial - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static string GerarPlaca()
    {
        var random = new Random();
        const string letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string alfanumerico = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        return string.Concat(
            letras[random.Next(letras.Length)],
            letras[random.Next(letras.Length)],
            letras[random.Next(letras.Length)],
            random.Next(0, 10),
            alfanumerico[random.Next(alfanumerico.Length)],
            random.Next(0, 10),
            random.Next(0, 10));
    }
}
