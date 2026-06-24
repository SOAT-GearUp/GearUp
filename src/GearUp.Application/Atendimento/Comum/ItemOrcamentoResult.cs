using GearUp.Domain.Enums;

namespace GearUp.Application.Atendimento.Comum;

public sealed record ItemOrcamentoResult(
    Guid Id,
    TipoItemOrcamento Tipo,
    string Descricao,
    decimal Quantidade,
    decimal ValorUnitario,
    decimal ValorTotal,
    Guid? EstoqueItemId);
