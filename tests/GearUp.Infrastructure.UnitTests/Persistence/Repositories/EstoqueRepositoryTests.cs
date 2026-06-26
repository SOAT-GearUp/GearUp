using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace GearUp.Infrastructure.UnitTests.Persistence.Repositories;

public sealed class EstoqueRepositoryTests
{
    [Fact]
    public async Task ObterAsync_DeveCarregarMovimentacoesDoItem()
    {
        await using var factory = new InMemoryDbContextFactory();
        using var scope = factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GearUpDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IEstoqueRepository>();
        var item = Estoque.Criar("Filtro de oleo", TipoItemEstoque.Peca, 45.90m, 10);

        await repository.AdicionarAsync(item, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var encontrado = await repository.ObterAsync(item.Id, CancellationToken.None);

        Assert.NotNull(encontrado);
        Assert.Equal(10, encontrado.QuantidadeDisponivel);
        Assert.Single(encontrado.Movimentacoes);
    }

    [Fact]
    public async Task ListarAsync_DeveOrdenarPorNome()
    {
        await using var factory = new InMemoryDbContextFactory();
        using var scope = factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GearUpDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IEstoqueRepository>();
        var itemB = Estoque.Criar("Oleo", TipoItemEstoque.Insumo, 30m, 5);
        var itemA = Estoque.Criar("Filtro", TipoItemEstoque.Peca, 45m, 2);

        await dbContext.EstoqueItens.AddRangeAsync(itemB, itemA);
        await dbContext.SaveChangesAsync();

        var itens = await repository.ListarAsync(CancellationToken.None);

        Assert.Collection(
            itens,
            item => Assert.Equal(itemA.Id, item.Id),
            item => Assert.Equal(itemB.Id, item.Id));
    }
}
