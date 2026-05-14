namespace VocabApp.Application.Users.Queries.GetStats;

public sealed record StatsDto(
    int TotalWords,
    int WordsLearnedThisWeek,
    float AverageEaseFactor);
