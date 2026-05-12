using VocabApp.Domain.Common;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Domain.Events;

public sealed record UserStreakUpdatedEvent(UserId UserId, int NewStreak) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
