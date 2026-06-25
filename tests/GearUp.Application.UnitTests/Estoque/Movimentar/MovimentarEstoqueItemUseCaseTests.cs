using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Application.Estoque.Movimentar;
using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Enums;
using EstoqueAggregate = GearUp.Domain.Entities.Estoque;

namespace GearUp.Application.UnitTests.Estoque.Movimentar;

public sealed class MovimentarEstoqueItemUseCaseTests
{
    [Fact]
    public async Task MovimentarAsync_ComItemInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var repository = new EstoqueRepositoryFake(item: null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new MovimentarEstoqueItemUseCase(repository, unitOfWork);
        var command = new MovimentarEstoqueItemCommand(Guid.NewGuid(), TipoMovimentacaoEstoque.Entrada, 5, "Compra");

        var ex = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.MovimentarAsync(command, CancellationToken.None));

        Assert.Equal("ITEM_ESTOQUE_NAO_ENCONTRADO", ex.Codigo);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task MovimentarAsync_ComEntrada_DeveAumentarSaldoESalvar()
    {
        var item = EstoqueAggregate.Criar("Filtro", TipoItemEstoque.Peca, 10m, 5);
        var repository = new EstoqueRepositoryFake(item);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new MovimentarEstoqueItemUseCase(repository, unitOfWork);
        var command = new MovimentarEstoqueItemCommand(item.Id, TipoMovimentacaoEstoque.Entrada, 3, "Compra");

        await useCase.MovimentarAsync(command, CancellationToken.None);

        Assert.Equal(8, item.QuantidadeDisponivel);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task MovimentarAsync_ComSaida_DeveReduzirSaldoESalvar()
    {
        var item = EstoqueAggregate.Criar("Filtro", TipoItemEstoque.Peca, 10m, 5);
        var repository = new EstoqueRepositoryFake(item);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new MovimentarEstoqueItemUseCase(repository, unitOfWork);
        var command = new MovimentarEstoqueItemCommand(item.Id, TipoMovimentacaoEstoque.Saida, 2, "Consumo na OS");

        await useCase.MovimentarAsync(command, CancellationToken.None);

        Assert.Equal(3, item.QuantidadeDisponivel);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task MovimentarAsync_ComSaidaSuperiorAoSaldo_DeveLancarRegraNegocio()
    {
        var item = EstoqueAggregate.Criar("Filtro", TipoItemEstoque.Peca, 10m, 1);
        var repository = new EstoqueRepositoryFake(item);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new MovimentarEstoqueItemUseCase(repository, unitOfWork);
        var command = new MovimentarEstoqueItemCommand(item.Id, TipoMovimentacaoEstoque.Saida, 5, "Consumo");

        var ex = await Assert.ThrowsAsync<RegraNegocioException>(
            () => useCase.MovimentarAsync(command, CancellationToken.None));

        Assert.Equal("ESTOQUE_INSUFICIENTE", ex.Codigo);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task MovimentarAsync_ComQuantidadeInvalida_DeveLancarArgumentException()
    {
        var item = EstoqueAggregate.Criar("Filtro", TipoItemEstoque.Peca, 10m, 5);
        var repository = new EstoqueRepositoryFake(item);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new MovimentarEstoqueItemUseCase(repository, unitOfWork);
        var command = new MovimentarEstoqueItemCommand(item.Id, TipoMovimentacaoEstoque.Entrada, 0, "Compra");

        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.MovimentarAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task MovimentarAsync_ComMotivoVazio_DeveLancarArgumentException()
    {
        var item = EstoqueAggregate.Criar("Filtro", TipoItemEstoque.Peca, 10m, 5);
        var repository = new EstoqueRepositoryFake(item);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new MovimentarEstoqueItemUseCase(repository, unitOfWork);
        var command = new MovimentarEstoqueItemCommand(item.Id, TipoMovimentacaoEstoque.Entrada, 2, "   ");

        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.MovimentarAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    private sealed class EstoqueRepositoryFake(EstoqueAggregate? item) : IEstoqueRepository
    {
        public Task AdicionarAsync(EstoqueAggregate item, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<EstoqueAggregate?> ObterAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(item);

        public Task<IReadOnlyList<EstoqueAggregate>> ListarAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<EstoqueAggregate>>([]);
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
