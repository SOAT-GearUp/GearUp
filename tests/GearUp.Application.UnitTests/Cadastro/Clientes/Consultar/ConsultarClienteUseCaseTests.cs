using GearUp.Application.Cadastro.Clientes.Common.Interfaces;
using GearUp.Application.Cadastro.Clientes.Consultar;
using GearUp.Application.Cadastro.Clientes.Veiculos.Common.Interfaces;
using GearUp.Application.Common.Exceptions;
using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects;

namespace GearUp.Application.UnitTests.Cadastro.Clientes.Consultar;

public sealed class ConsultarClienteUseCaseTests
{
    [Fact]
    public async Task ObterAsync_ComClienteExistente_DeveRetornarClienteComVeiculos()
    {
        var cliente = CriarCliente();
        var veiculo = Veiculo.Criar(cliente.Id, "ABC1D23", "Fiat", "Uno", 2020);
        var clienteRepository = new ClienteRepositoryFake(cliente);
        var veiculoRepository = new VeiculoRepositoryFake([veiculo]);
        var useCase = new ConsultarClienteUseCase(clienteRepository, veiculoRepository);

        var result = await useCase.ObterAsync(cliente.Id, CancellationToken.None);

        Assert.Equal(cliente.Id, result.Id);
        Assert.Equal("Maria da Silva", result.Nome);
        Assert.Equal("Cpf", result.TipoDocumento);
        var unicoVeiculo = Assert.Single(result.Veiculos);
        Assert.Equal(veiculo.Id, unicoVeiculo.Id);
        Assert.Equal("ABC1D23", unicoVeiculo.Placa);
    }

    [Fact]
    public async Task ObterAsync_ComClienteSemVeiculos_DeveRetornarListaDeVeiculosVazia()
    {
        var cliente = CriarCliente();
        var clienteRepository = new ClienteRepositoryFake(cliente);
        var veiculoRepository = new VeiculoRepositoryFake([]);
        var useCase = new ConsultarClienteUseCase(clienteRepository, veiculoRepository);

        var result = await useCase.ObterAsync(cliente.Id, CancellationToken.None);

        Assert.Empty(result.Veiculos);
    }

    [Fact]
    public async Task ObterAsync_ComClienteInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var clienteRepository = new ClienteRepositoryFake(null);
        var veiculoRepository = new VeiculoRepositoryFake([]);
        var useCase = new ConsultarClienteUseCase(clienteRepository, veiculoRepository);

        var ex = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.ObterAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("CLIENTE_NAO_ENCONTRADO", ex.Codigo);
    }

    private static Cliente CriarCliente() =>
        Cliente.Criar("Maria da Silva", "52998224725", "maria@email.com", "11999999999");

    private sealed class ClienteRepositoryFake(Cliente? cliente) : IClienteRepository
    {
        public Task<Cliente?> ObterAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(cliente);
        public Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Cliente>>([]);
        public Task<bool> DocumentoExisteAsync(Documento documento, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class VeiculoRepositoryFake(IReadOnlyList<Veiculo> veiculos) : IVeiculoRepository
    {
        public Task<Veiculo?> ObterAsync(Guid id, CancellationToken ct) => Task.FromResult<Veiculo?>(null);
        public Task<IReadOnlyList<Veiculo>> ListarPorClienteAsync(Guid clienteId, CancellationToken ct) => Task.FromResult(veiculos);
        public Task AdicionarAsync(Veiculo veiculo, CancellationToken ct) => Task.CompletedTask;
        public Task<bool> PlacaExisteAsync(string placa, Guid? ignorarId, CancellationToken ct) => Task.FromResult(false);
    }
}
