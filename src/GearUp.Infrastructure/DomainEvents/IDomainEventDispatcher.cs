using GearUp.Domain.Common.DomainEvents;

namespace GearUp.Infrastructure.DomainEvents
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
    }
}
