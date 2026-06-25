using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Application.Estoque.Listar;
using GearUp.Domain.Enums;
using EstoqueAggregate = GearUp.Domain.Entities.Estoque;

namespace GearUp.Application.UnitTests.Estoque.Listar;

public sealed class ListarEstoqueItemUseCaseTests
{
    [Fact]
    public async Task ListarAsync_SemItens_DeveRetornarListaVazia()
    {
        var repository = new EstoqueRepositoryFake([]);
        var useCase = new ListarEstoqueItemUseCase(repository);

        var result = await useCase.ListarAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListarAsync_ComItens_DeveMapearCadaCampo()
    {
        var peca = EstoqueAggregate.Criar("Filtro de óleo", TipoItemEstoque.Peca, 49.90m, 10);
        var insumo = EstoqueAggregate.Criar("Óleo 5W30", TipoItemEstoque.Insumo, 30m, 4);
        var repository = new EstoqueRepositoryFake([peca, insumo]);
        var useCase = new ListarEstoqueItemUseCase(repository);

        var result = await useCase.ListarAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);

        var primeiro = result[0];
        Assert.Equal(peca.Id, primeiro.Id);
        Assert.Equal("Filtro de óleo", primeiro.Nome);
        Assert.Equal("Peca", primeiro.Descricao);
        Assert.Equal(10, primeiro.QuantidadeDisponivel);
        Assert.Equal(49.90m, primeiro.PrecoUnitario);

        var segundo = result[1];
        Assert.Equal(insumo.Id, segundo.Id);
        Assert.Equal("Insumo", segundo.Descricao);
        Assert.Equal(4, segundo.QuantidadeDisponivel);
    }

    private sealed class EstoqueRepositoryFake(IReadOnlyList<EstoqueAggregate> itens) : IEstoqueRepository
    {
        public Task AdicionarAsync(EstoqueAggregate item, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<EstoqueAggregate?> ObterAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<EstoqueAggregate?>(null);

        public Task<IReadOnlyList<EstoqueAggregate>> ListarAsync(CancellationToken cancellationToken) =>
            Task.FromResult(itens);
    }
}
