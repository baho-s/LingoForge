using VocabApp.Domain.Common;
using VocabApp.Domain.Enums;
using VocabApp.Domain.Events;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Domain.Aggregates.WordAggregate;

public sealed class Word : AggregateRoot<WordId>
{
    public UserId OwnerId { get; private set; } = default!;
    public string Original { get; private set; } = string.Empty;
    public string Translation { get; private set; } = string.Empty;
    public string? AiSentence { get; private set; }
    public ReviewInfo Review { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private Word() { }

    private Word(WordId id, UserId ownerId, string original, string translation)
        : base(id)
    {
        OwnerId = ownerId;
        Original = original;
        Translation = translation;
        CreatedAt = DateTime.UtcNow;
        Review = new ReviewInfo(
            IntervalDays: 0,
            EaseFactor: 2.5f,
            Repetitions: 0,
            NextReviewAt: DateTime.UtcNow
        );
    }

    public static Word Create(UserId ownerId, string original, string translation)
    {
        if (string.IsNullOrWhiteSpace(original))
        {
            throw new ArgumentException("Original word is required.", nameof(original));
        }

        if (string.IsNullOrWhiteSpace(translation))
        {
            throw new ArgumentException("Translation is required.", nameof(translation));
        }

        return new Word(new WordId(Guid.NewGuid()), ownerId, original.Trim(), translation.Trim());
    }

    public void AttachAiSentence(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
        {
            throw new ArgumentException("Sentence is required.", nameof(sentence));
        }

        AiSentence = sentence.Trim();
    }

    public void RecordReview(ReviewOutcome outcome)
    {
        var nextReview = CalculateNextReview(outcome, Review);
        Review = nextReview;
        AddDomainEvent(new WordReviewedEvent(Id, OwnerId, outcome));
    }

    private static ReviewInfo CalculateNextReview(ReviewOutcome outcome, ReviewInfo current)
    {
        var repetitions = current.Repetitions;
        var easeFactor = current.EaseFactor;
        var intervalDays = current.IntervalDays;

        if (outcome == ReviewOutcome.Again)
        {
            repetitions = 0;
            intervalDays = 1;
            easeFactor = Math.Max(1.3f, easeFactor - 0.2f);
        }
        else
        {
            repetitions += 1;
            easeFactor = outcome switch
            {
                ReviewOutcome.Hard => Math.Max(1.3f, easeFactor - 0.15f),
                ReviewOutcome.Good => easeFactor,
                ReviewOutcome.Easy => easeFactor + 0.15f,
                _ => easeFactor,
            };

            intervalDays = repetitions switch
            {
                1 => 1,
                2 => 6,
                _ => Math.Max(1, (int)Math.Round(intervalDays * easeFactor)),
            };
        }

        var nextReviewAt = DateTime.UtcNow.AddDays(intervalDays);
        return new ReviewInfo(intervalDays, easeFactor, repetitions, nextReviewAt);
    }
}
