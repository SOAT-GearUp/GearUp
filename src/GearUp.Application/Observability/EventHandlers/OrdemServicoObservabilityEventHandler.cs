using GearUp.Application.Common.DomainEvents;
using GearUp.Application.Common.Observability;
using GearUp.Domain.DomainEvents.Atendimento;

namespace GearUp.Application.Observability.EventHandlers;

internal sealed class OrdemServicoObservabilityEventHandler(
    IOrdemServicoMetrics metrics) :
    IDomainEventHandler<OrdemServicoCriadaDomainEvent>,
    IDomainEventHandler<StatusOrdemServicoAlteradoDomainEvent>
{
    public Task HandleAsync(
        OrdemServicoCriadaDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        metrics.RegistrarCriacao(domainEvent.OrdemServicoId, domainEvent.OcorridoEm);
        return Task.CompletedTask;
    }

    public Task HandleAsync(
        StatusOrdemServicoAlteradoDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        metrics.RegistrarTransicaoStatus(
            domainEvent.OrdemServicoId,
            domainEvent.StatusAnterior,
            domainEvent.StatusAtual,
            domainEvent.TempoNoStatusAnterior,
            domainEvent.OcorridoEm);

        return Task.CompletedTask;
    }
}
