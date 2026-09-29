using GearUp.Domain.Common.DomainEvents;
using GearUp.Domain.Enums;

namespace GearUp.Domain.DomainEvents.Atendimento;

public sealed record StatusOrdemServicoAlteradoDomainEvent(
    Guid OrdemServicoId,
    StatusOrdemServico StatusAnterior,
    StatusOrdemServico StatusAtual,
    TimeSpan TempoNoStatusAnterior,
    DateTimeOffset OcorridoEm) : IDomainEvent;
