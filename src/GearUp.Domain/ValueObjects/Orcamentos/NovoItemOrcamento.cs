using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Enums;

namespace GearUp.Domain.ValueObjects.Orcamentos;

public sealed record NovoItemOrcamento
{
    private NovoItemOrcamento(TipoItemOrcamento tipo, string descricao, decimal quantidade, decimal valorUnitario, Guid? estoqueItemId)
    {
        Tipo = tipo;
        Descricao = descricao;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
        EstoqueItemId = estoqueItemId;
    }

    public TipoItemOrcamento Tipo { get; }
    public string Descricao { get; }
    public decimal Quantidade { get; }
    public decimal ValorUnitario { get; }
    public Guid? EstoqueItemId { get; }

    public static NovoItemOrcamento Criar(TipoItemOrcamento tipo, string descricao, decimal quantidade, decimal valorUnitario, Guid? estoqueItemId)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException(
                "A descrição é obrigatória.");

        if (quantidade <= 0)
            throw new ArgumentException(
                "A quantidade deve ser maior que zero.");

        if (valorUnitario < 0)
            throw new ArgumentException(
                "O valor unitário não pode ser negativo.");

        if ((tipo is TipoItemOrcamento.Peca or TipoItemOrcamento.Insumo) && estoqueItemId is null)
            throw new RegraNegocioException(
                "ESTOQUE_ITEM_OBRIGATORIO",
                "Peças e insumos devem estar vinculados a um item de estoque.");

        if ((tipo is TipoItemOrcamento.Servico or TipoItemOrcamento.MaoDeObra) && estoqueItemId is not null)
            throw new RegraNegocioException(
                "ESTOQUE_ITEM_INVALIDO",
                "Serviços e mão de obra não devem estar vinculados a item de estoque.");

        return new NovoItemOrcamento(
            tipo,
            descricao.Trim(),
            quantidade,
            decimal.Round(valorUnitario, 2),
            estoqueItemId);
    }
}