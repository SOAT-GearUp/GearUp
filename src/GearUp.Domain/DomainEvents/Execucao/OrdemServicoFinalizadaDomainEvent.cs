using GearUp.Domain.Common.DomainEvents;

namespace GearUp.Domain.DomainEvents.Execucao;

public sealed record OrdemServicoFinalizadaDomainEvent(
    Guid OrdemServicoId,
    Guid ClienteId,
    DateTimeOffset OcorridoEm) : IDomainEvent;
