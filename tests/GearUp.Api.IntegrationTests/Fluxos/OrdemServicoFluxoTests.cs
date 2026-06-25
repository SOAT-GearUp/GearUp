using System.Net;
using System.Net.Http.Json;
using GearUp.Api.Contracts.Clientes.Cadastrar;
using GearUp.Api.Contracts.Clientes.Veiculos.Cadastrar;
using GearUp.Api.Contracts.Orcamentos;
using GearUp.Api.Contracts.OrdemServico;
using GearUp.Api.IntegrationTests.Infrastructure;
using GearUp.Application.Atendimento.Clientes.Cadastrar;
using GearUp.Application.Atendimento.Clientes.Veiculos.Cadastrar;
using GearUp.Application.Atendimento.Consultar;
using GearUp.Application.Atendimento.Criar;
using GearUp.Domain.Enums;

namespace GearUp.Api.IntegrationTests.Fluxos;

/// <summary>
/// Testes de integração do fluxo de Ordem de Serviço, exercitando os controllers
/// <c>ClientesController</c>, <c>VeiculosController</c> e <c>OrdensServicoController</c>
/// de ponta a ponta via HTTP real: cadastro de cliente e veículo, criação da OS,
/// alteração de status e consulta, validando status codes e o corpo das respostas.
/// </summary>
public sealed class OrdemServicoFluxoTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task FluxoCompleto_DeCadastroAteCancelamento_DevePersistirCadaEtapa()
    {
        var client = await CriarClienteAutenticadoAsync("atendente");

        // 1. Cadastrar cliente e veículo.
        var clienteId = await CadastrarClienteAsync(client);
        var veiculoId = await CadastrarVeiculoAsync(client, clienteId);

        // 2. Criar a OS — espera 201 com header Location e Id no corpo.
        const string solicitacao = "Cliente relata barulho na suspensão dianteira.";
        var criar = await client.PostAsJsonAsync(
            "/api/ordens-servico",
            new CriarOrdemServicoRequest(clienteId, veiculoId, solicitacao, PrioridadeOrdemServico.Alta, Prazo: null));

        Assert.Equal(HttpStatusCode.Created, criar.StatusCode);

        var osCriada = await criar.Content.ReadFromJsonAsync<CriarOrdemServicoResult>();
        Assert.NotNull(osCriada);
        Assert.NotEqual(Guid.Empty, osCriada!.Id);
        Assert.Equal($"/api/ordens-servico/{osCriada.Id}", criar.Headers.Location?.ToString());

        // 3. Consultar a OS recém-criada — deve estar em Recebida com os dados informados.
        var recebida = await ConsultarAsync(client, osCriada.Id);
        Assert.Equal(StatusOrdemServico.Recebida, recebida.Status);
        Assert.Equal(clienteId, recebida.ClienteId);
        Assert.Equal(veiculoId, recebida.VeiculoId);
        Assert.Equal(solicitacao, recebida.SolicitacaoInicial);
        Assert.Equal(PrioridadeOrdemServico.Alta, recebida.Prioridade);

        // 4. Alterar o status da OS (Recebida -> Cancelada) — espera 204.
        var alterar = await client.PatchAsJsonAsync(
            $"/api/ordens-servico/{osCriada.Id}/status",
            new AlterarStatusRequest(StatusOrdemServico.Cancelada));

        Assert.Equal(HttpStatusCode.NoContent, alterar.StatusCode);

        // 5. Consultar novamente — a transição e os dados devem ter persistido.
        var cancelada = await ConsultarAsync(client, osCriada.Id);
        Assert.Equal(StatusOrdemServico.Cancelada, cancelada.Status);
        Assert.Equal(solicitacao, cancelada.SolicitacaoInicial);
        Assert.Contains(cancelada.Historico, h => h.Tipo == "OS_CANCELADA");
    }

    [Fact]
    public async Task AlterarStatus_TransicaoInvalidaDeRecebidaParaFinalizada_DeveRetornar422()
    {
        var client = await CriarClienteAutenticadoAsync("atendente");

        var clienteId = await CadastrarClienteAsync(client);
        var veiculoId = await CadastrarVeiculoAsync(client, clienteId);

        var criar = await client.PostAsJsonAsync(
            "/api/ordens-servico",
            new CriarOrdemServicoRequest(clienteId, veiculoId, "Revisão geral.", PrioridadeOrdemServico.Normal, Prazo: null));
        Assert.Equal(HttpStatusCode.Created, criar.StatusCode);
        var osCriada = await criar.Content.ReadFromJsonAsync<CriarOrdemServicoResult>();
        Assert.NotNull(osCriada);

        // Finalizada não é alcançável a partir de Recebida (somente EmExecucao -> Finalizada).
        var alterar = await client.PatchAsJsonAsync(
            $"/api/ordens-servico/{osCriada!.Id}/status",
            new AlterarStatusRequest(StatusOrdemServico.Finalizada));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, alterar.StatusCode);

        // A OS deve permanecer em Recebida após a regra de negócio rejeitar a transição.
        var inalterada = await ConsultarAsync(client, osCriada.Id);
        Assert.Equal(StatusOrdemServico.Recebida, inalterada.Status);
    }

    private static async Task<Guid> CadastrarClienteAsync(HttpClient client)
    {
        var sufixo = Guid.NewGuid().ToString("N")[..8];
        var resposta = await client.PostAsJsonAsync(
            "/api/clientes",
            new CadastrarClienteRequest(
                Nome: $"Oficina Cliente {sufixo}",
                Documento: GerarCpf(),
                Email: $"cliente.{sufixo}@gearup.test",
                Telefone: "11999990000"));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var criado = await resposta.Content.ReadFromJsonAsync<CadastrarClienteResult>();
        Assert.NotNull(criado);
        Assert.NotEqual(Guid.Empty, criado!.Id);

        return criado.Id;
    }

    private static async Task<Guid> CadastrarVeiculoAsync(HttpClient client, Guid clienteId)
    {
        var resposta = await client.PostAsJsonAsync(
            $"/api/clientes/{clienteId}/veiculos",
            new CadastrarVeiculoRequest(Placa: GerarPlaca(), Marca: "Volkswagen", Modelo: "Gol", Ano: 2020));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var criado = await resposta.Content.ReadFromJsonAsync<CadastrarVeiculoResult>();
        Assert.NotNull(criado);
        Assert.NotEqual(Guid.Empty, criado!.VeiculoId);

        return criado.VeiculoId;
    }

    private static async Task<ConsultarOrdemServicoResult> ConsultarAsync(HttpClient client, Guid ordemServicoId)
    {
        var resposta = await client.GetAsync($"/api/ordens-servico/{ordemServicoId}");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var os = await resposta.Content.ReadFromJsonAsync<ConsultarOrdemServicoResult>();
        Assert.NotNull(os);

        return os!;
    }

    /// <summary>Gera um CPF válido (com dígitos verificadores corretos) e único por chamada.</summary>
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

    /// <summary>Gera uma placa válida no formato Mercosul (LLLNLNN).</summary>
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
