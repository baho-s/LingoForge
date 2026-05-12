using MediatR;
using VocabApp.Application.Common.Events;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Events;

namespace VocabApp.Application.Words.Events;

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
