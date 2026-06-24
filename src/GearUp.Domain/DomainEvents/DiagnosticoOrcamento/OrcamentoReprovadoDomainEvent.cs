using GearUp.Domain.Common.DomainEvents;

namespace GearUp.Domain.DomainEvents.DiagnosticoOrcamento;

public sealed record OrcamentoReprovadoDomainEvent(
    Guid OrdemServicoId,
    Guid ClienteId,
    Guid OrcamentoId,
    DateTimeOffset OcorridoEm) : IDomainEvent;
