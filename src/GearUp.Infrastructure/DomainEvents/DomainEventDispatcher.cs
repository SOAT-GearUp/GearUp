using GearUp.Application.Common.DomainEvents;
using GearUp.Domain.Common.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace GearUp.Infrastructure.DomainEvents
{
    internal sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
    {
        public async Task DispatchAsync(
            IReadOnlyCollection<IDomainEvent> domainEvents,
            CancellationToken cancellationToken)
        {
            foreach (var domainEvent in domainEvents)
            {
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
                var handlers = serviceProvider.GetServices(handlerType);

                foreach (var handler in handlers)
                {
                    var task = (Task)handlerType
                        .GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!
                        .Invoke(handler, [domainEvent, cancellationToken])!;

                    await task;
                }
            }
        }
    }
}
