namespace BeeZillion.Application.Users.Queries.GetStats;

public sealed record ActivityHeatmapDay(DateOnly Date, int ActivityCount);

public sealed record StatsDto(
    int TotalWords,
    int WordsLearnedThisWeek,
    float AverageEaseFactor,
    int TotalAttempts,
    int CorrectAttempts,
    float AccuracyRate,
    long AverageTimeTakenMs,
    int CorrectAttemptsThisWeek,
    int TodayReviewedWords,
    int TodayAttempts,
    int TodayCorrectAttempts,
    IReadOnlyList<ActivityHeatmapDay> ActivityHeatmap);

