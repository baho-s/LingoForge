using BeeZillion.Domain.Aggregates.WordAggregate;

namespace BeeZillion.Application.Words.Dtos;

public sealed record WordDto(
    Guid Id,
    string Original,
    string Translation,
    string? AiSentence,
    string? Field,
    int IntervalDays,
    float EaseFactor,
    int Repetitions,
    DateTime NextReviewAt,
    DateTime CreatedAt)
{
    public static WordDto FromEntity(Word word)
    {
        return new WordDto(
            word.Id.Value,
            word.Original,
            word.Translation,
            word.AiSentence,
            word.Field,
            word.Review.IntervalDays,
            word.Review.EaseFactor,
            word.Review.Repetitions,
            word.Review.NextReviewAt,
            word.CreatedAt);
    }
}

