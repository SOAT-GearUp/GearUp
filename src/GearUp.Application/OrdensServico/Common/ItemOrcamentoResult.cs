using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Application.OrdensServico.Common
{
    public sealed record ItemOrcamentoResult(
        Guid Id,
        TipoItemOrcamento Tipo,
        string Descricao,
        decimal Quantidade,
        decimal ValorUnitario,
        decimal ValorTotal,
        Guid? EstoqueItemId);
}
