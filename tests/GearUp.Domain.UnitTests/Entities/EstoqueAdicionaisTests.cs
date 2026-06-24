using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.DomainEvents.Estoque;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Domain.UnitTests.Entities;

public sealed class EstoqueAdicionaisTests
{
    [Fact]
    public void Atualizar_ComDadosValidos_DeveAlterarNomeEPreco()
    {
        var item = Estoque.Criar("Filtro", TipoItemEstoque.Peca, 30);

        item.Atualizar("Filtro de Ar", 45.50m);

        Assert.Equal("Filtro de Ar", item.Nome);
        Assert.Equal(45.50m, item.PrecoUnitario);
    }

    [Fact]
    public void Criar_ComQuantidadeInicialNegativa_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() =>
            Estoque.Criar("Filtro", TipoItemEstoque.Peca, 30, -1));
    }

    [Fact]
    public void Criar_ComPrecoNegativo_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() =>
            Estoque.Criar("Filtro", TipoItemEstoque.Peca, -1));
    }

    [Fact]
    public void Criar_ComNomeVazio_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() =>
            Estoque.Criar("   ", TipoItemEstoque.Peca, 30));
    }

    [Fact]
    public void Movimentar_ComQuantidadeZero_DeveFalhar()
    {
        var item = Estoque.Criar("Filtro", TipoItemEstoque.Peca, 30, 10);

        Assert.Throws<ArgumentException>(() =>
            item.Movimentar(TipoMovimentacaoEstoque.Saida, 0, "Uso"));
    }

    [Fact]
    public void Movimentar_ComQuantidadeNegativa_DeveFalhar()
    {
        var item = Estoque.Criar("Filtro", TipoItemEstoque.Peca, 30, 10);

        Assert.Throws<ArgumentException>(() =>
            item.Movimentar(TipoMovimentacaoEstoque.Entrada, -1, "Entrada"));
    }

    [Fact]
    public void Movimentar_ComMotivoVazio_DeveFalhar()
    {
        var item = Estoque.Criar("Filtro", TipoItemEstoque.Peca, 30, 10);

        Assert.Throws<ArgumentException>(() =>
            item.Movimentar(TipoMovimentacaoEstoque.Saida, 1, ""));
    }

    [Fact]
    public void Movimentar_Entrada_DeveAdicionarDomainEvent()
    {
        var item = Estoque.Criar("Filtro", TipoItemEstoque.Peca, 30);

        item.Movimentar(TipoMovimentacaoEstoque.Entrada, 5, "Reposição");

        Assert.Equal(5, item.QuantidadeDisponivel);
        Assert.Contains(item.DomainEvents.OfType<EstoqueItemMovimentadoDomainEvent>(), e =>
            e.TipoMovimentacao == TipoMovimentacaoEstoque.Entrada && e.Quantidade == 5);
    }

    [Fact]
    public void LimparDomainEvents_DeveRemoverTodosOsEventos()
    {
        var item = Estoque.Criar("Filtro", TipoItemEstoque.Peca, 30, 5);
        Assert.NotEmpty(item.DomainEvents);

        item.LimparDomainEvents();

        Assert.Empty(item.DomainEvents);
    }

    [Fact]
    public void Movimentar_ComOrdemServicoId_DeveAssociarAOS()
    {
        var item = Estoque.Criar("Óleo", TipoItemEstoque.Insumo, 50, 10);
        var osId = Guid.NewGuid();

        item.Movimentar(TipoMovimentacaoEstoque.Saida, 2, "Uso em serviço", osId);

        var movimentacao = item.Movimentacoes.Last();
        Assert.Equal(osId, movimentacao.OrdemServicoId);
    }

    [Fact]
    public void Atualizar_ComNomeVazio_DeveFalhar()
    {
        var item = Estoque.Criar("Filtro", TipoItemEstoque.Peca, 30);

        Assert.Throws<ArgumentException>(() => item.Atualizar("", 25));
    }

    [Fact]
    public void Atualizar_ComPrecoNegativo_DeveFalhar()
    {
        var item = Estoque.Criar("Filtro", TipoItemEstoque.Peca, 30);

        Assert.Throws<ArgumentException>(() => item.Atualizar("Filtro", -5));
    }

    [Fact]
    public void Criar_ComInsumo_DeveDefinirTipoCorreto()
    {
        var item = Estoque.Criar("Óleo 5W30", TipoItemEstoque.Insumo, 50);

        Assert.Equal(TipoItemEstoque.Insumo, item.Tipo);
        Assert.True(item.Ativo);
        Assert.Equal(0, item.QuantidadeDisponivel);
    }
}
