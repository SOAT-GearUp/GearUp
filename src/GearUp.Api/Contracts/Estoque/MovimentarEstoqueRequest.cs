using GearUp.Domain.Enums;

namespace GearUp.Api.Contracts.Estoque
{
    public sealed record MovimentarEstoqueRequest(
        TipoMovimentacaoEstoque Tipo, 
        decimal Quantidade, 
        string Motivo);
}
