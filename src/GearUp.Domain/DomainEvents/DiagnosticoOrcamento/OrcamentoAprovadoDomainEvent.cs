using GearUp.Domain.Common.DomainEvents;

namespace GearUp.Domain.DomainEvents.DiagnosticoOrcamento;

public sealed record OrcamentoAprovadoDomainEvent(
    Guid OrdemServicoId,
    Guid ClienteId,
    Guid OrcamentoId,
    DateTimeOffset OcorridoEm) : IDomainEvent;
