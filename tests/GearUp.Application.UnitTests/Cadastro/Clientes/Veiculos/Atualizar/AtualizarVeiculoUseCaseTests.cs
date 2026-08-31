using GearUp.Application.Cadastro.Veiculos.Atualizar;
using GearUp.Application.Cadastro.Veiculos.Common.Interfaces;
using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Domain.Entities;

namespace GearUp.Application.UnitTests.Cadastro.Clientes.Veiculos.Atualizar;

public sealed class AtualizarVeiculoUseCaseTests
{
    [Fact]
    public async Task AtualizarVeiculoAsync_ComDadosValidos_DeveAtualizarESalvar()
    {
        var clienteId = Guid.NewGuid();
        var veiculo = Veiculo.Criar(clienteId, "ABC1D23", "Fiat", "Uno", 2020);
        var veiculoRepository = new VeiculoRepositoryFake(veiculo, placaExiste: false);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AtualizarVeiculoUseCase(veiculoRepository, unitOfWork);
        var command = new AtualizarVeiculoCommand(clienteId, veiculo.Id, "XYZ2A34", "Volkswagen", "Gol", 2022);

        await useCase.AtualizarVeiculoAsync(command, CancellationToken.None);

        Assert.Equal("XYZ2A34", veiculo.Placa);
        Assert.Equal("Volkswagen", veiculo.Marca);
        Assert.Equal("Gol", veiculo.Modelo);
        Assert.Equal(2022, veiculo.Ano);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task AtualizarVeiculoAsync_ComVeiculoInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var veiculoRepository = new VeiculoRepositoryFake(null, placaExiste: false);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AtualizarVeiculoUseCase(veiculoRepository, unitOfWork);
        var command = new AtualizarVeiculoCommand(Guid.NewGuid(), Guid.NewGuid(), "XYZ2A34", "Volkswagen", "Gol", 2022);

        var ex = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.AtualizarVeiculoAsync(command, CancellationToken.None));

        Assert.Equal("VEICULO_NAO_ENCONTRADO", ex.Codigo);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task AtualizarVeiculoAsync_ComVeiculoDeOutroCliente_DeveLancarRecursoNaoEncontrado()
    {
        var veiculo = Veiculo.Criar(Guid.NewGuid(), "ABC1D23", "Fiat", "Uno", 2020);
        var veiculoRepository = new VeiculoRepositoryFake(veiculo, placaExiste: false);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AtualizarVeiculoUseCase(veiculoRepository, unitOfWork);
        var command = new AtualizarVeiculoCommand(Guid.NewGuid(), veiculo.Id, "XYZ2A34", "Volkswagen", "Gol", 2022);

        var ex = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.AtualizarVeiculoAsync(command, CancellationToken.None));

        Assert.Equal("VEICULO_NAO_ENCONTRADO", ex.Codigo);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task AtualizarVeiculoAsync_ComPlacaDuplicada_DeveLancarConflito()
    {
        var clienteId = Guid.NewGuid();
        var veiculo = Veiculo.Criar(clienteId, "ABC1D23", "Fiat", "Uno", 2020);
        var veiculoRepository = new VeiculoRepositoryFake(veiculo, placaExiste: true);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AtualizarVeiculoUseCase(veiculoRepository, unitOfWork);
        var command = new AtualizarVeiculoCommand(clienteId, veiculo.Id, "XYZ2A34", "Volkswagen", "Gol", 2022);

        var ex = await Assert.ThrowsAsync<ConflitoException>(
            () => useCase.AtualizarVeiculoAsync(command, CancellationToken.None));

        Assert.Equal("PLACA_DUPLICADA", ex.Codigo);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    private sealed class VeiculoRepositoryFake(Veiculo? veiculo, bool placaExiste) : IVeiculoRepository
    {
        public Task<Veiculo?> ObterAsync(Guid id, CancellationToken ct) => Task.FromResult(veiculo);
        public Task<IReadOnlyList<Veiculo>> ListarPorClienteAsync(Guid clienteId, CancellationToken ct) => Task.FromResult<IReadOnlyList<Veiculo>>([]);
        public Task AdicionarAsync(Veiculo veiculo, CancellationToken ct) => Task.CompletedTask;
        public Task<bool> PlacaExisteAsync(string placa, Guid? ignorarId, CancellationToken ct) => Task.FromResult(placaExiste);
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
