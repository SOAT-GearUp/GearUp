using GearUp.Domain.Enums;

namespace GearUp.Application.Estoque.Movimentar
{
    public sealed record MovimentarEstoqueItemCommand(
        Guid Id,
        TipoMovimentacaoEstoque Tipo,
        decimal Quantidade,
        string Motivo);
}