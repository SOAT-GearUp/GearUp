using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Domain.UnitTests.ValueObjects;

public sealed class NovoItemOrcamentoTests
{
    [Fact]
    public void Criar_ComDescricaoVazia_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() =>
            NovoItemOrcamento.Criar(TipoItemOrcamento.Servico, "", 1, 100, null));
    }

    [Fact]
    public void Criar_ComDescricaoEspacos_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() =>
            NovoItemOrcamento.Criar(TipoItemOrcamento.Servico, "   ", 1, 100, null));
    }

    [Fact]
    public void Criar_ComQuantidadeZero_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() =>
            NovoItemOrcamento.Criar(TipoItemOrcamento.Servico, "Alinhamento", 0, 100, null));
    }

    [Fact]
    public void Criar_ComQuantidadeNegativa_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() =>
            NovoItemOrcamento.Criar(TipoItemOrcamento.Servico, "Alinhamento", -1, 100, null));
    }

    [Fact]
    public void Criar_ComValorNegativo_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() =>
            NovoItemOrcamento.Criar(TipoItemOrcamento.Servico, "Alinhamento", 1, -0.01m, null));
    }

    [Fact]
    public void Criar_ServicoComEstoqueItemId_DeveFalhar()
    {
        var ex = Assert.Throws<RegraNegocioException>(() =>
            NovoItemOrcamento.Criar(TipoItemOrcamento.Servico, "Alinhamento", 1, 100, Guid.NewGuid()));

        Assert.Equal("ESTOQUE_ITEM_INVALIDO", ex.Codigo);
    }

    [Fact]
    public void Criar_MaoDeObraComEstoqueItemId_DeveFalhar()
    {
        var ex = Assert.Throws<RegraNegocioException>(() =>
            NovoItemOrcamento.Criar(TipoItemOrcamento.MaoDeObra, "Mão de obra", 1, 50, Guid.NewGuid()));

        Assert.Equal("ESTOQUE_ITEM_INVALIDO", ex.Codigo);
    }

    [Fact]
    public void Criar_PecaComDadosValidos_DeveCriar()
    {
        var estoqueId = Guid.NewGuid();

        var item = NovoItemOrcamento.Criar(TipoItemOrcamento.Peca, " Filtro de óleo ", 2, 45.555m, estoqueId);

        Assert.Equal(TipoItemOrcamento.Peca, item.Tipo);
        Assert.Equal("Filtro de óleo", item.Descricao);
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(45.56m, item.ValorUnitario);
        Assert.Equal(estoqueId, item.EstoqueItemId);
    }

    [Fact]
    public void Criar_InsumoComDadosValidos_DeveCriar()
    {
        var item = NovoItemOrcamento.Criar(TipoItemOrcamento.Insumo, "Óleo 5W30", 1, 80, Guid.NewGuid());

        Assert.Equal(TipoItemOrcamento.Insumo, item.Tipo);
        Assert.NotNull(item.EstoqueItemId);
    }

    [Fact]
    public void Criar_ServicoSemEstoqueItem_DeveCriar()
    {
        var item = NovoItemOrcamento.Criar(TipoItemOrcamento.Servico, "Revisão", 1, 150, null);

        Assert.Equal(TipoItemOrcamento.Servico, item.Tipo);
        Assert.Null(item.EstoqueItemId);
    }

    [Fact]
    public void Criar_ValorZero_DeveSerPermitido()
    {
        var item = NovoItemOrcamento.Criar(TipoItemOrcamento.Servico, "Revisão gratuita", 1, 0, null);

        Assert.Equal(0, item.ValorUnitario);
    }

    [Fact]
    public void Criar_MaoDeObraSemEstoqueItem_DeveCriar()
    {
        var item = NovoItemOrcamento.Criar(TipoItemOrcamento.MaoDeObra, "Troca de pneu", 2, 60, null);

        Assert.Equal(TipoItemOrcamento.MaoDeObra, item.Tipo);
        Assert.Equal(2, item.Quantidade);
    }
}
