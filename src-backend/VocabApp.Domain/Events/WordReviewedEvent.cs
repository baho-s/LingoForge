using VocabApp.Domain.Common;
using VocabApp.Domain.Enums;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Domain.Events;

public sealed record WordReviewedEvent(WordId WordId, UserId UserId, ReviewOutcome Outcome) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
