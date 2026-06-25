using GearUp.Application.Common.Interfaces;
using GearUp.Application.Estoque.Cadastrar;
using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Domain.Enums;
using EstoqueAggregate = GearUp.Domain.Entities.Estoque;

namespace GearUp.Application.UnitTests.Estoque.Cadastrar;

public sealed class CadastrarEstoqueItemUseCaseTests
{
    [Fact]
    public async Task CadastrarAsync_ComDadosValidos_DeveAdicionarESalvar()
    {
        var repository = new EstoqueRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CadastrarEstoqueItemUseCase(repository, unitOfWork);
        var command = new CadastrarEstoqueItemCommand("Filtro de óleo", TipoItemEstoque.Peca, 49.90m, 10);

        var result = await useCase.CadastrarAsync(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotNull(repository.ItemAdicionado);
        Assert.Equal(result.Id, repository.ItemAdicionado!.Id);
        Assert.Equal("Filtro de óleo", repository.ItemAdicionado.Nome);
        Assert.Equal(10, repository.ItemAdicionado.QuantidadeDisponivel);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CadastrarAsync_SemQuantidadeInicial_DeveCadastrarComSaldoZero()
    {
        var repository = new EstoqueRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CadastrarEstoqueItemUseCase(repository, unitOfWork);
        var command = new CadastrarEstoqueItemCommand("Óleo 5W30", TipoItemEstoque.Insumo, 30m);

        var result = await useCase.CadastrarAsync(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(0, repository.ItemAdicionado!.QuantidadeDisponivel);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CadastrarAsync_ComNomeVazio_DeveLancarArgumentException()
    {
        var repository = new EstoqueRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CadastrarEstoqueItemUseCase(repository, unitOfWork);
        var command = new CadastrarEstoqueItemCommand("   ", TipoItemEstoque.Peca, 10m);

        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.CadastrarAsync(command, CancellationToken.None));

        Assert.Null(repository.ItemAdicionado);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CadastrarAsync_ComQuantidadeInicialNegativa_DeveLancarArgumentException()
    {
        var repository = new EstoqueRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CadastrarEstoqueItemUseCase(repository, unitOfWork);
        var command = new CadastrarEstoqueItemCommand("Filtro", TipoItemEstoque.Peca, 10m, -1);

        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.CadastrarAsync(command, CancellationToken.None));

        Assert.Null(repository.ItemAdicionado);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CadastrarAsync_ComPrecoNegativo_DeveLancarArgumentException()
    {
        var repository = new EstoqueRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CadastrarEstoqueItemUseCase(repository, unitOfWork);
        var command = new CadastrarEstoqueItemCommand("Filtro", TipoItemEstoque.Peca, -5m);

        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.CadastrarAsync(command, CancellationToken.None));

        Assert.Null(repository.ItemAdicionado);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    private sealed class EstoqueRepositoryFake : IEstoqueRepository
    {
        public EstoqueAggregate? ItemAdicionado { get; private set; }

        public Task AdicionarAsync(EstoqueAggregate item, CancellationToken cancellationToken)
        {
            ItemAdicionado = item;
            return Task.CompletedTask;
        }

        public Task<EstoqueAggregate?> ObterAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<EstoqueAggregate?>(null);

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
