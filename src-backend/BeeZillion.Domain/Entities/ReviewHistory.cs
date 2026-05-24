using BeeZillion.Domain.Common;
using BeeZillion.Domain.Enums;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Domain.Entities;

public sealed class ReviewHistory : Entity<ReviewHistoryId>
{
    public UserId UserId { get; private set; }
    public WordId WordId { get; private set; }
    public bool IsCorrect { get; private set; }
    public ReviewOutcome Outcome { get; private set; }
    public int? QScore { get; private set; }
    public long? TimeTakenMs { get; private set; }
    public DateTime ReviewedAt { get; private set; }
    public DateTime NextReviewAt { get; private set; }
    public int IntervalDays { get; private set; }
    public float EaseFactor { get; private set; }
    public int Repetitions { get; private set; }
    public ReviewSource Source { get; private set; }
    public Guid? SessionId { get; private set; }
    public string? ClientVersion { get; private set; }

    private ReviewHistory()
    {
        UserId = null!;
        WordId = null!;
    }

    private ReviewHistory(
        ReviewHistoryId id,
        UserId userId,
        WordId wordId,
        bool isCorrect,
        ReviewOutcome outcome,
        int? qScore,
        long? timeTakenMs,
        DateTime reviewedAt,
        DateTime nextReviewAt,
        int intervalDays,
        float easeFactor,
        int repetitions,
        ReviewSource source,
        Guid? sessionId,
        string? clientVersion)
        : base(id)
    {
        UserId = userId;
        WordId = wordId;
        IsCorrect = isCorrect;
        Outcome = outcome;
        QScore = qScore;
        TimeTakenMs = timeTakenMs;
        ReviewedAt = reviewedAt;
        NextReviewAt = nextReviewAt;
        IntervalDays = intervalDays;
        EaseFactor = easeFactor;
        Repetitions = repetitions;
        Source = source;
        SessionId = sessionId;
        ClientVersion = clientVersion;
    }

    public static ReviewHistory Create(
        UserId userId,
        WordId wordId,
        bool isCorrect,
        ReviewOutcome outcome,
        int? qScore,
        long? timeTakenMs,
        ReviewInfo review,
        ReviewSource source,
        Guid? sessionId = null,
        string? clientVersion = null,
        DateTime? reviewedAt = null)
    {
        return new ReviewHistory(
            new ReviewHistoryId(Guid.NewGuid()),
            userId,
            wordId,
            isCorrect,
            outcome,
            qScore,
            timeTakenMs,
            reviewedAt ?? DateTime.UtcNow,
            review.NextReviewAt,
            review.IntervalDays,
            review.EaseFactor,
            review.Repetitions,
            source,
            sessionId,
            clientVersion);
    }
}

