using GearUp.Domain.Enums;

namespace GearUp.Application.Atendimento.Listar;

public sealed record ListarOrdemServicoResult(
    Guid Id,
    Guid ClienteId,
    Guid VeiculoId,
    StatusOrdemServico Status,
    PrioridadeOrdemServico Prioridade,
    DateTimeOffset CriadaEm,
    DateTimeOffset? Prazo);
