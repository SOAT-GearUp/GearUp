using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using GearUp.Api.IntegrationTests.Infrastructure;

namespace GearUp.Api.IntegrationTests.Fluxos;

/// <summary>
/// Testes de integração HTTP cobrindo o CRUD de <c>ClientesController</c> e os
/// endpoints disponíveis de <c>VeiculosController</c> (POST e PUT). Como o
/// banco é compartilhado entre os testes da mesma classe, cada teste usa
/// documento e placa únicos para não colidir.
/// </summary>
public sealed class ClientesVeiculosCrudTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    private const string Atendente = "atendente";

    private static int _contadorDocumento;
    private static int _contadorPlaca;

    // ----------------------------------------------------------------- Clientes

    [Fact]
    public async Task CadastrarCliente_ComDadosValidos_DeveRetornar201ComLocation()
    {
        var client = await CriarClienteAutenticadoAsync(Atendente);

        var resposta = await client.PostAsJsonAsync("/api/clientes", NovoClienteRequest());

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(resposta.Headers.Location);

        var corpo = await resposta.Content.ReadFromJsonAsync<CadastrarClienteResposta>();
        Assert.NotNull(corpo);
        Assert.NotEqual(Guid.Empty, corpo!.Id);
    }

    [Fact]
    public async Task ObterCliente_QuandoExiste_DeveRetornar200ComDados()
    {
        var client = await CriarClienteAutenticadoAsync(Atendente);
        var request = NovoClienteRequest();
        var id = await CadastrarClienteAsync(client, request);

        var resposta = await client.GetAsync($"/api/clientes/{id}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var cliente = await resposta.Content.ReadFromJsonAsync<ConsultarClienteResposta>();
        Assert.NotNull(cliente);
        Assert.Equal(id, cliente!.Id);
        Assert.Equal(request.Nome, cliente.Nome);
        Assert.Contains(request.Email, cliente.Email);
        Assert.Contains(request.Documento, cliente.Documento);
    }

    [Fact]
    public async Task ListarClientes_QuandoAutenticado_DeveRetornar200ComClienteCadastrado()
    {
        var client = await CriarClienteAutenticadoAsync(Atendente);
        var id = await CadastrarClienteAsync(client, NovoClienteRequest());

        var resposta = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var clientes = await resposta.Content.ReadFromJsonAsync<List<ListarClienteResposta>>();
        Assert.NotNull(clientes);
        Assert.Contains(clientes!, c => c.Id == id);
    }

    [Fact]
    public async Task AtualizarCliente_QuandoExiste_DeveRetornar204()
    {
        var client = await CriarClienteAutenticadoAsync(Atendente);
        var id = await CadastrarClienteAsync(client, NovoClienteRequest());

        var atualizacao = new
        {
            Nome = "Nome Atualizado",
            Email = "atualizado@example.com",
            Telefone = "11988887777",
        };

        var resposta = await client.PutAsJsonAsync($"/api/clientes/{id}", atualizacao);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);

        var cliente = await client.GetFromJsonAsync<ConsultarClienteResposta>($"/api/clientes/{id}");
        Assert.NotNull(cliente);
        Assert.Equal(atualizacao.Nome, cliente!.Nome);
        Assert.Contains(atualizacao.Email, cliente.Email);
    }

    [Fact]
    public async Task ExcluirCliente_QuandoExiste_DeveRetornar204()
    {
        var client = await CriarClienteAutenticadoAsync(Atendente);
        var id = await CadastrarClienteAsync(client, NovoClienteRequest());

        var resposta = await client.DeleteAsync($"/api/clientes/{id}");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);

        var aposExclusao = await client.GetAsync($"/api/clientes/{id}");
        Assert.Equal(HttpStatusCode.NotFound, aposExclusao.StatusCode);
    }

    [Fact]
    public async Task ObterCliente_QuandoNaoExiste_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync(Atendente);

        var resposta = await client.GetAsync($"/api/clientes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task AtualizarCliente_QuandoNaoExiste_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync(Atendente);

        var atualizacao = new
        {
            Nome = "Inexistente",
            Email = "inexistente@example.com",
            Telefone = "11999990000",
        };

        var resposta = await client.PutAsJsonAsync($"/api/clientes/{Guid.NewGuid()}", atualizacao);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task ExcluirCliente_QuandoNaoExiste_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync(Atendente);

        var resposta = await client.DeleteAsync($"/api/clientes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    // ----------------------------------------------------------------- Veículos

    [Fact]
    public async Task AdicionarVeiculo_ComDadosValidos_DeveRetornar201()
    {
        var client = await CriarClienteAutenticadoAsync(Atendente);
        var clienteId = await CadastrarClienteAsync(client, NovoClienteRequest());

        var placa = GerarPlaca();
        var veiculo = new
        {
            Placa = placa,
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2020,
        };

        var resposta = await client.PostAsJsonAsync($"/api/clientes/{clienteId}/veiculos", veiculo);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<CadastrarVeiculoResposta>();
        Assert.NotNull(corpo);
        Assert.NotEqual(Guid.Empty, corpo!.VeiculoId);

        // Consulta/listagem indireta: o veículo aparece na coleção do cliente.
        var cliente = await client.GetFromJsonAsync<ConsultarClienteResposta>($"/api/clientes/{clienteId}");
        Assert.NotNull(cliente);
        Assert.Contains(cliente!.Veiculos, v => v.Id == corpo.VeiculoId && v.Placa == placa);
    }

    [Fact]
    public async Task AtualizarVeiculo_QuandoExiste_DeveRetornar204()
    {
        var client = await CriarClienteAutenticadoAsync(Atendente);
        var clienteId = await CadastrarClienteAsync(client, NovoClienteRequest());

        var criacao = new
        {
            Placa = GerarPlaca(),
            Marca = "Fiat",
            Modelo = "Uno",
            Ano = 2015,
        };
        var criado = await client.PostAsJsonAsync($"/api/clientes/{clienteId}/veiculos", criacao);
        criado.EnsureSuccessStatusCode();
        var veiculoId = (await criado.Content.ReadFromJsonAsync<CadastrarVeiculoResposta>())!.VeiculoId;

        var novaPlaca = GerarPlaca();
        var atualizacao = new
        {
            Placa = novaPlaca,
            Marca = "Volkswagen",
            Modelo = "Gol",
            Ano = 2018,
        };

        var resposta = await client.PutAsJsonAsync(
            $"/api/clientes/{clienteId}/veiculos/{veiculoId}", atualizacao);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);

        var cliente = await client.GetFromJsonAsync<ConsultarClienteResposta>($"/api/clientes/{clienteId}");
        Assert.NotNull(cliente);
        var veiculo = Assert.Single(cliente!.Veiculos, v => v.Id == veiculoId);
        Assert.Equal(novaPlaca, veiculo.Placa);
        Assert.Equal(atualizacao.Modelo, veiculo.Modelo);
        Assert.Equal(atualizacao.Ano, veiculo.Ano);
    }

    [Fact]
    public async Task AdicionarVeiculo_QuandoClienteNaoExiste_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync(Atendente);

        var veiculo = new
        {
            Placa = GerarPlaca(),
            Marca = "Honda",
            Modelo = "Civic",
            Ano = 2021,
        };

        var resposta = await client.PostAsJsonAsync($"/api/clientes/{Guid.NewGuid()}/veiculos", veiculo);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task AtualizarVeiculo_QuandoNaoExiste_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync(Atendente);
        var clienteId = await CadastrarClienteAsync(client, NovoClienteRequest());

        var atualizacao = new
        {
            Placa = GerarPlaca(),
            Marca = "Renault",
            Modelo = "Kwid",
            Ano = 2022,
        };

        var resposta = await client.PutAsJsonAsync(
            $"/api/clientes/{clienteId}/veiculos/{Guid.NewGuid()}", atualizacao);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    // ------------------------------------------------------------------ Helpers

    private static ClienteRequest NovoClienteRequest() => new(
        Nome: "Cliente de Teste",
        Documento: GerarCpf(),
        Email: $"cliente-{Guid.NewGuid():N}@example.com",
        Telefone: "11999998888");

    private static async Task<Guid> CadastrarClienteAsync(HttpClient client, ClienteRequest request)
    {
        var resposta = await client.PostAsJsonAsync("/api/clientes", request);
        resposta.EnsureSuccessStatusCode();

        var corpo = await resposta.Content.ReadFromJsonAsync<CadastrarClienteResposta>();
        return corpo!.Id;
    }

    /// <summary>
    /// Gera um CPF de 11 dígitos com dígitos verificadores válidos. Usa um
    /// contador estático como base para garantir unicidade dentro da classe.
    /// </summary>
    private static string GerarCpf()
    {
        var sequencial = Interlocked.Increment(ref _contadorDocumento);
        // Base de 9 dígitos a partir do sequencial (com offset para evitar
        // padrões com todos os dígitos iguais, que são rejeitados).
        var baseNumero = (100_000_000 + sequencial) % 1_000_000_000;
        var digitos = baseNumero.ToString("D9", CultureInfo.InvariantCulture)
            .Select(c => c - '0')
            .ToArray();

        var primeiro = CalcularDigitoVerificador(digitos, 10);
        var segundo = CalcularDigitoVerificador([.. digitos, primeiro], 11);

        return string.Concat(digitos.Select(d => d.ToString(CultureInfo.InvariantCulture)))
            + primeiro.ToString(CultureInfo.InvariantCulture)
            + segundo.ToString(CultureInfo.InvariantCulture);
    }

    private static int CalcularDigitoVerificador(IReadOnlyList<int> digitos, int pesoInicial)
    {
        var soma = 0;
        var peso = pesoInicial;
        foreach (var digito in digitos)
        {
            soma += digito * peso;
            peso--;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    /// <summary>
    /// Gera uma placa única no formato Mercosul aceito pelo domínio:
    /// <c>^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$</c>.
    /// </summary>
    private static string GerarPlaca()
    {
        var sequencial = Interlocked.Increment(ref _contadorPlaca);

        var letras = new char[3];
        var valor = sequencial;
        for (var i = 0; i < 3; i++)
        {
            letras[i] = (char)('A' + (valor % 26));
            valor /= 26;
        }

        var primeiroDigito = sequencial % 10;
        var letraMeio = (char)('A' + (sequencial % 26));
        var ultimosDigitos = sequencial % 100;

        return new string(letras)
            + primeiroDigito.ToString(CultureInfo.InvariantCulture)
            + letraMeio
            + ultimosDigitos.ToString("D2", CultureInfo.InvariantCulture);
    }

    // ----------------------------------------------- Records de desserialização

    private sealed record ClienteRequest(string Nome, string Documento, string Email, string Telefone);

    private sealed record CadastrarClienteResposta(Guid Id);

    private sealed record ListarClienteResposta(Guid Id, string Nome, string Email, string Telefone);

    private sealed record ConsultarClienteResposta(
        Guid Id,
        string Nome,
        string Documento,
        string TipoDocumento,
        string Email,
        string Telefone,
        IReadOnlyList<VeiculoResposta> Veiculos);

    private sealed record VeiculoResposta(Guid Id, string Placa, string Marca, string Modelo, int Ano);

    private sealed record CadastrarVeiculoResposta(Guid VeiculoId);
}
