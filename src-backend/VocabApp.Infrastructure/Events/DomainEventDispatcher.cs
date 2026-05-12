using MediatR;
using VocabApp.Application.Common.Events;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Common;

namespace VocabApp.Infrastructure.Events;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;

    public DomainEventDispatcher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent);
            if (notification is null)
            {
                continue;
            }

            await _publisher.Publish((INotification)notification, ct);
        }
    }
}
