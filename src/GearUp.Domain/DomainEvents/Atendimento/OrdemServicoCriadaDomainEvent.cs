using GearUp.Domain.Common.DomainEvents;

namespace GearUp.Domain.DomainEvents.Atendimento;

public sealed record OrdemServicoCriadaDomainEvent(
    Guid OrdemServicoId,
    Guid ClienteId,
    Guid VeiculoId,
    DateTimeOffset OcorridoEm) : IDomainEvent;
