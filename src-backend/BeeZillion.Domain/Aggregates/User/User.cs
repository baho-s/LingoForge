using BeeZillion.Domain.Common;
using BeeZillion.Domain.Entities;
using BeeZillion.Domain.Enums;
using BeeZillion.Domain.Events;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Domain.Aggregates.UserAggregate;

public sealed class User : AggregateRoot<UserId>
{
    private readonly List<Badge> _badges = new();

    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public int Streak { get; private set; }
    public int DailyGoal { get; private set; }
    public DateTime LastActivity { get; private set; }
    public int ReviewCount { get; private set; }
    public IReadOnlyList<Badge> Badges => _badges.AsReadOnly();

    private User() { }

    private User(UserId id, string email, string passwordHash)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        DailyGoal = 10;
        Streak = 0;
        LastActivity = DateTime.UtcNow;
        ReviewCount = 0;
    }

    public static User Create(string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        return new User(new UserId(Guid.NewGuid()), email.Trim().ToLowerInvariant(), passwordHash);
    }

    public void RecordActivity(DateTime utcNow)
    {
        var today = utcNow.Date;
        var lastDay = LastActivity.Date;

        var newStreak = Streak;
        if (lastDay == today)
        {
            return;
        }

        if (lastDay == today.AddDays(-1))
        {
            newStreak = Streak + 1;
        }
        else
        {
            newStreak = 1;
        }

        Streak = newStreak;
        LastActivity = utcNow;
        AddDomainEvent(new UserStreakUpdatedEvent(Id, Streak));
    }

    public int RecordReview(DateTime utcNow)
    {
        ReviewCount += 1;
        RecordActivity(utcNow);
        return ReviewCount;
    }

    public void UpdateDailyGoal(int dailyGoal)
    {
        if (dailyGoal <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dailyGoal), "Daily goal must be positive.");
        }

        DailyGoal = dailyGoal;
    }

    public void AwardBadge(BadgeType badgeType)
    {
        if (_badges.Any(b => b.Type == badgeType))
        {
            return;
        }

        var badge = new Badge(badgeType, DateTime.UtcNow);
        _badges.Add(badge);
        AddDomainEvent(new BadgeEarnedEvent(Id, badgeType));
    }
}

