using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Domain.UnitTests.Entities;

public sealed class EstoqueItemTests
{
    [Fact]
    public void CriarComQuantidadeInicial_DeveRegistrarEntradaInicial()
    {
        var item = EstoqueItem.Criar("Filtro", TipoItemEstoque.Peca, 30, 5);

        Assert.Equal(5, item.QuantidadeDisponivel);
        var movimentacao = Assert.Single(item.Movimentacoes);
        Assert.Equal(TipoMovimentacaoEstoque.Entrada, movimentacao.Tipo);
        Assert.Equal("Saldo inicial", movimentacao.Motivo);
    }

    [Fact]
    public void EntradaESaida_DevemAtualizarSaldoEAuditoria()
    {
        var item = EstoqueItem.Criar("Óleo 5W30", TipoItemEstoque.Insumo, 50);
        item.Movimentar(TipoMovimentacaoEstoque.Entrada, 10, "Compra");
        item.Movimentar(TipoMovimentacaoEstoque.Saida, 2, "Uso em serviço", Guid.NewGuid());
        Assert.Equal(8, item.QuantidadeDisponivel);
        Assert.Equal(2, item.Movimentacoes.Count);
    }

    [Fact]
    public void SaidaAcimaDoSaldo_DeveFalhar()
    {
        var item = EstoqueItem.Criar("Filtro", TipoItemEstoque.Peca, 30);
        var erro = Assert.Throws<RegraNegocioException>(() => item.Movimentar(TipoMovimentacaoEstoque.Saida, 1, "Uso"));
        Assert.Equal("ESTOQUE_INSUFICIENTE", erro.Codigo);
    }
}
