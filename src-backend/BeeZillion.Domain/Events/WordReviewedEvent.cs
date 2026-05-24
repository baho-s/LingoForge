using BeeZillion.Domain.Common;
using BeeZillion.Domain.Enums;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Domain.Events;

public sealed record WordReviewedEvent(WordId WordId, UserId UserId, ReviewOutcome Outcome) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

