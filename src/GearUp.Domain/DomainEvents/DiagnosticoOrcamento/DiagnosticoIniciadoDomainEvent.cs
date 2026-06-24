using GearUp.Domain.Common.DomainEvents;

namespace GearUp.Domain.DomainEvents.DiagnosticoOrcamento;

public sealed record DiagnosticoIniciadoDomainEvent(
    Guid OrdemServicoId,
    Guid MecanicoId,
    DateTimeOffset OcorridoEm) : IDomainEvent;
