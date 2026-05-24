using MediatR;
using BeeZillion.Application.Common.Events;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Domain.Events;

namespace BeeZillion.Application.Words.Events;

public sealed class WordReviewedCacheInvalidationHandler
    : INotificationHandler<DomainEventNotification<WordReviewedEvent>>
{
    private readonly ICacheService _cacheService;

    public WordReviewedCacheInvalidationHandler(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public Task Handle(DomainEventNotification<WordReviewedEvent> notification, CancellationToken cancellationToken)
    {
        var userId = notification.DomainEvent.UserId;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cacheKey = $"wotd:{userId.Value}:{today}";
        return _cacheService.RemoveAsync(cacheKey, cancellationToken);
    }
}

