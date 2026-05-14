using VocabApp.Domain.Enums;

namespace VocabApp.Application.Users.Queries.GetDashboard;

public sealed record BadgeDto(BadgeType Type, DateTime AwardedAt);

public sealed record WeeklyActivityPoint(DateOnly Date, int WordsAdded);

public sealed record DashboardDto(
    int Streak,
    int DailyGoal,
    int ReviewCount,
    DateTime LastActivity,
    IReadOnlyList<BadgeDto> Badges,
    IReadOnlyList<WeeklyActivityPoint> WeeklyActivity);
