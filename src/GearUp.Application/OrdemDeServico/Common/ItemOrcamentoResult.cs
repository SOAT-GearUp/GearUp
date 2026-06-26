using GearUp.Domain.Enums;

namespace GearUp.Application.OrdemDeServico.Common;

public sealed record ItemOrcamentoResult(
    Guid Id,
    TipoItemOrcamento Tipo,
    string Descricao,
    decimal Quantidade,
    decimal ValorUnitario,
    decimal ValorTotal,
    Guid? EstoqueItemId);
