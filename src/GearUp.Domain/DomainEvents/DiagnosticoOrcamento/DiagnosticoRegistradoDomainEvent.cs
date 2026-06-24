using GearUp.Domain.Common.DomainEvents;

namespace GearUp.Domain.DomainEvents.DiagnosticoOrcamento;

public sealed record DiagnosticoRegistradoDomainEvent(
    Guid OrdemServicoId,
    string Diagnostico,
    DateTimeOffset OcorridoEm) : IDomainEvent;
