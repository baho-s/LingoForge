using VocabApp.Domain.Enums;

namespace VocabApp.Application.Users.Queries.GetDashboard;

public sealed record BadgeDto(BadgeType Type, DateTime AwardedAt);

public sealed record WeeklyActivityPoint(DateOnly Date, int ReviewedCount);

public sealed record DashboardDto(
    int Streak,
    int WeeklyGoal,
    int ReviewedThisWeek,
    DateTime LastActivity,
    IReadOnlyList<BadgeDto> Badges,
    IReadOnlyList<WeeklyActivityPoint> WeeklyActivity);
