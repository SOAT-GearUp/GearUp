using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Domain.UnitTests.Entities;

public sealed class OrcamentoTests
{
    private static NovoItemOrcamento ItemServico(string descricao = "Alinhamento", decimal qty = 1, decimal valor = 100)
        => NovoItemOrcamento.Criar(TipoItemOrcamento.Servico, descricao, qty, valor, null);

    private static NovoItemOrcamento ItemPeca(string descricao = "Filtro", decimal qty = 1, decimal valor = 50)
        => NovoItemOrcamento.Criar(TipoItemOrcamento.Peca, descricao, qty, valor, Guid.NewGuid());

    [Fact]
    public void Criar_ComListaVazia_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() => Orcamento.Criar(Guid.NewGuid(), 1, []));
    }

    [Fact]
    public void Decidir_Aprovado_DeveAlterarStatusERegistrarData()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), 1, [ItemServico()]);

        orcamento.Decidir(true);

        Assert.Equal(StatusOrcamento.Aprovado, orcamento.Status);
        Assert.NotNull(orcamento.DecididoEm);
    }

    [Fact]
    public void Decidir_Reprovado_DeveAlterarStatus()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), 1, [ItemServico()]);

        orcamento.Decidir(false);

        Assert.Equal(StatusOrcamento.Rejeitado, orcamento.Status);
        Assert.NotNull(orcamento.DecididoEm);
    }

    [Fact]
    public void Decidir_JaDecidido_DeveFalhar()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), 1, [ItemServico()]);
        orcamento.Decidir(true);

        var ex = Assert.Throws<RegraNegocioException>(() => orcamento.Decidir(false));
        Assert.Equal("ORCAMENTO_JA_DECIDIDO", ex.Codigo);
    }

    [Fact]
    public void AtualizarItem_ItemNaoEncontrado_DeveFalhar()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), 1, [ItemServico()]);

        var ex = Assert.Throws<RegraNegocioException>(() =>
            orcamento.AtualizarItem(Guid.NewGuid(), ItemServico("Novo")));

        Assert.Equal("ITEM_ORCAMENTO_NAO_ENCONTRADO", ex.Codigo);
    }

    [Fact]
    public void RemoverItem_ItemNaoEncontrado_DeveFalhar()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), 1, [ItemServico()]);

        var ex = Assert.Throws<RegraNegocioException>(() =>
            orcamento.RemoverItem(Guid.NewGuid()));

        Assert.Equal("ITEM_ORCAMENTO_NAO_ENCONTRADO", ex.Codigo);
    }

    [Fact]
    public void ValorTotal_DeveCalcularSomaDosItens()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), 1, [
            ItemServico("Serv 1", 2, 50),
            ItemPeca("Peca 1", 3, 30)
        ]);

        Assert.Equal(190, orcamento.ValorTotal);
    }

    [Fact]
    public void Criar_ComItemPeca_DeveVincularEstoqueItem()
    {
        var estoqueId = Guid.NewGuid();
        var item = NovoItemOrcamento.Criar(TipoItemOrcamento.Peca, "Filtro de óleo", 2, 45, estoqueId);

        var orcamento = Orcamento.Criar(Guid.NewGuid(), 1, [item]);

        var itemAdicionado = Assert.Single(orcamento.Itens);
        Assert.Equal(estoqueId, itemAdicionado.EstoqueItemId);
        Assert.Equal(90, itemAdicionado.ValorTotal);
    }

    [Fact]
    public void AdicionarItem_OrcamentoDecidido_DeveFalhar()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), 1, [ItemServico()]);
        orcamento.Decidir(true);

        var ex = Assert.Throws<RegraNegocioException>(() => orcamento.AdicionarItem(ItemServico("Novo")));
        Assert.Equal("ORCAMENTO_JA_DECIDIDO", ex.Codigo);
    }

    [Fact]
    public void RemoverItem_OrcamentoDecidido_DeveFalhar()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), 1, [ItemServico()]);
        var item = Assert.Single(orcamento.Itens);
        orcamento.Decidir(false);

        var ex = Assert.Throws<RegraNegocioException>(() => orcamento.RemoverItem(item.Id));
        Assert.Equal("ORCAMENTO_JA_DECIDIDO", ex.Codigo);
    }

    [Fact]
    public void AtualizarItem_OrcamentoDecidido_DeveFalhar()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), 1, [ItemServico()]);
        var item = Assert.Single(orcamento.Itens);
        orcamento.Decidir(true);

        var ex = Assert.Throws<RegraNegocioException>(() =>
            orcamento.AtualizarItem(item.Id, ItemServico("Novo")));
        Assert.Equal("ORCAMENTO_JA_DECIDIDO", ex.Codigo);
    }

    [Fact]
    public void Criar_ComMultiplosItens_DeveArmazenarTodos()
    {
        var itens = new[]
        {
            ItemServico("Balanceamento", 4, 25),
            ItemPeca("Pneu", 4, 250),
            NovoItemOrcamento.Criar(TipoItemOrcamento.MaoDeObra, "Troca de pneus", 1, 80, null)
        };

        var orcamento = Orcamento.Criar(Guid.NewGuid(), 1, itens);

        Assert.Equal(3, orcamento.Itens.Count);
        Assert.Equal(1180, orcamento.ValorTotal);
    }
}
