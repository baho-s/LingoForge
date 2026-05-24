using MediatR;
using BeeZillion.Application.Common.Exceptions;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Domain.Repositories;

namespace BeeZillion.Application.Users.Queries.GetDashboard;

public sealed class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserVocabularyProgressRepository _progressRepository;
    private readonly ICurrentUserService _currentUser;

    public GetDashboardQueryHandler(
        IUserRepository userRepository,
        IUserVocabularyProgressRepository progressRepository,
        ICurrentUserService currentUser)
    {
        _userRepository = userRepository;
        _progressRepository = progressRepository;
        _currentUser = currentUser;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        // Haftalık hedef hesapla: (135, 180, 240, 300) döngüsü
        var weekNumber = (int)((DateTime.UtcNow - new DateTime(2026, 1, 1)).TotalDays / 7) + 1;
        var cycleWeek = ((weekNumber - 1) % 4) + 1;
        var weeklyGoal = cycleWeek switch
        {
            1 => 135,
            2 => 180,
            3 => 240,
            4 => 300,
            _ => 135
        };

        // Bu hafta review'lenen kelimeleri al
        var userProgresses = await _progressRepository.GetByUserAsync(userId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = today.AddDays(-6);

        var dailyReviewedCounts = new Dictionary<DateOnly, int>();
        for (var i = 0; i < 7; i++)
        {
            dailyReviewedCounts[weekStart.AddDays(i)] = 0;
        }

        // Her progress'in TotalAttempts'i var, şu hafta içinde attempt kayıtlı olanları say
        foreach (var progress in userProgresses)
        {
            var attemptDate = DateOnly.FromDateTime(progress.UpdatedAt);
            if (attemptDate >= weekStart && attemptDate <= today)
            {
                if (dailyReviewedCounts.ContainsKey(attemptDate))
                {
                    dailyReviewedCounts[attemptDate]++;
                }
            }
        }

        var reviewedThisWeek = dailyReviewedCounts.Values.Sum();

        var weekly = new List<WeeklyActivityPoint>(7);
        for (var i = 0; i < 7; i++)
        {
            var date = weekStart.AddDays(i);
            dailyReviewedCounts.TryGetValue(date, out var count);
            weekly.Add(new WeeklyActivityPoint(date, count));
        }

        var badges = user.Badges
            .Select(badge => new BadgeDto(badge.Type, badge.AwardedAt))
            .ToList();

        return new DashboardDto(
            user.Streak,
            weeklyGoal,
            reviewedThisWeek,
            user.LastActivity,
            badges,
            weekly);
    }
}

