using GearUp.Application.Cadastro.Clientes.Common.Interfaces;
using GearUp.Application.Cadastro.Clientes.Veiculos.Cadastrar;
using GearUp.Application.Cadastro.Veiculos.Common.Interfaces;
using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects.Clientes;

namespace GearUp.Application.UnitTests.Cadastro.Clientes.Veiculos.Cadastrar;

public sealed class CadastrarVeiculoUseCaseTests
{
    [Fact]
    public async Task CadastrarVeiculoAsync_ComPlacaNovaEClienteExistente_DeveAdicionarESalvar()
    {
        var cliente = CriarCliente();
        var clienteRepository = new ClienteRepositoryFake(cliente);
        var veiculoRepository = new VeiculoRepositoryFake(placaExiste: false);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CadastrarVeiculoUseCase(clienteRepository, veiculoRepository, unitOfWork);
        var command = new CadastrarVeiculoCommand(cliente.Id, "ABC1D23", "Fiat", "Uno", 2020);

        var result = await useCase.CadastrarVeiculoAsync(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.VeiculoId);
        Assert.NotNull(veiculoRepository.VeiculoAdicionado);
        Assert.Equal(result.VeiculoId, veiculoRepository.VeiculoAdicionado.Id);
        Assert.Equal(cliente.Id, veiculoRepository.VeiculoAdicionado.ClienteId);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CadastrarVeiculoAsync_ComPlacaDuplicada_DeveLancarConflito()
    {
        var cliente = CriarCliente();
        var clienteRepository = new ClienteRepositoryFake(cliente);
        var veiculoRepository = new VeiculoRepositoryFake(placaExiste: true);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CadastrarVeiculoUseCase(clienteRepository, veiculoRepository, unitOfWork);
        var command = new CadastrarVeiculoCommand(cliente.Id, "ABC1D23", "Fiat", "Uno", 2020);

        var ex = await Assert.ThrowsAsync<ConflitoException>(
            () => useCase.CadastrarVeiculoAsync(command, CancellationToken.None));

        Assert.Equal("PLACA_DUPLICADA", ex.Codigo);
        Assert.Null(veiculoRepository.VeiculoAdicionado);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CadastrarVeiculoAsync_ComClienteInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var clienteRepository = new ClienteRepositoryFake(null);
        var veiculoRepository = new VeiculoRepositoryFake(placaExiste: false);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CadastrarVeiculoUseCase(clienteRepository, veiculoRepository, unitOfWork);
        var command = new CadastrarVeiculoCommand(Guid.NewGuid(), "ABC1D23", "Fiat", "Uno", 2020);

        var ex = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.CadastrarVeiculoAsync(command, CancellationToken.None));

        Assert.Equal("CLIENTE_NAO_ENCONTRADO", ex.Codigo);
        Assert.Null(veiculoRepository.VeiculoAdicionado);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
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

    private sealed class VeiculoRepositoryFake(bool placaExiste) : IVeiculoRepository
    {
        public Veiculo? VeiculoAdicionado { get; private set; }

        public Task<Veiculo?> ObterAsync(Guid id, CancellationToken ct) => Task.FromResult<Veiculo?>(null);
        public Task<IReadOnlyList<Veiculo>> ListarPorClienteAsync(Guid clienteId, CancellationToken ct) => Task.FromResult<IReadOnlyList<Veiculo>>([]);
        public Task<bool> PlacaExisteAsync(string placa, Guid? ignorarId, CancellationToken ct) => Task.FromResult(placaExiste);

        public Task AdicionarAsync(Veiculo veiculo, CancellationToken ct)
        {
            VeiculoAdicionado = veiculo;
            return Task.CompletedTask;
        }
    }

    private sealed class UnitOfWorkFake : IUnitOfWork
    {
        public int SaveChangesChamadas { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesChamadas++;
            return Task.FromResult(1);
        }
    }
}
