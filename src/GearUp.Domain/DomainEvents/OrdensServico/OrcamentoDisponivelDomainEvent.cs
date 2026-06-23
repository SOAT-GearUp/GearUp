using GearUp.Domain.Common.DomainEvents;

namespace GearUp.Domain.DomainEvents.OrdensServico
{
    public sealed record OrcamentoDisponivelDomainEvent(
        Guid OrdemServicoId,
        Guid ClienteId,
        Guid OrcamentoId,
        int Versao,
        DateTimeOffset OcorridoEm)
        : IDomainEvent;
}
