using GearUp.Domain.Enums;

namespace GearUp.Application.OrdemDeServico.Common;

public sealed record OrcamentoResult(
    Guid Id,
    int Versao,
    StatusOrcamento Status,
    decimal ValorTotal,
    DateTimeOffset CriadoEm,
    DateTimeOffset? DecididoEm,
    IReadOnlyCollection<ItemOrcamentoResult> Itens);
