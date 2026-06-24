using GearUp.Domain.Enums;

namespace GearUp.Application.Atendimento.Comum;

public sealed record OrcamentoResult(
    Guid Id,
    int Versao,
    StatusOrcamento Status,
    decimal ValorTotal,
    DateTimeOffset CriadoEm,
    DateTimeOffset? DecididoEm,
    IReadOnlyCollection<ItemOrcamentoResult> Itens);
