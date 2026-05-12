namespace VocabApp.Application.Users.Queries.GetStats;

public sealed record StatsDto(
    int TotalWords,
    int WordsDueToday,
    int WordsWithAiSentence,
    int WordsWithoutAiSentence,
    int TotalReviews);
