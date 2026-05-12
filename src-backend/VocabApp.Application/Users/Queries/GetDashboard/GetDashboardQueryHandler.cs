using MediatR;
using VocabApp.Application.Common.Exceptions;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.Users.Queries.GetDashboard;

public sealed class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUser;

    public GetDashboardQueryHandler(
        IUserRepository userRepository,
        IWordRepository wordRepository,
        ICurrentUserService currentUser)
    {
        _userRepository = userRepository;
        _wordRepository = wordRepository;
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

        var words = await _wordRepository.GetByOwnerAsync(userId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = today.AddDays(-6);

        var counts = words
            .Select(word => DateOnly.FromDateTime(word.CreatedAt))
            .Where(date => date >= start && date <= today)
            .GroupBy(date => date)
            .ToDictionary(group => group.Key, group => group.Count());

        var weekly = new List<WeeklyActivityPoint>(7);
        for (var i = 0; i < 7; i += 1)
        {
            var date = start.AddDays(i);
            counts.TryGetValue(date, out var count);
            weekly.Add(new WeeklyActivityPoint(date, count));
        }

        var badges = user.Badges
            .Select(badge => new BadgeDto(badge.Type, badge.AwardedAt))
            .ToList();

        return new DashboardDto(
            user.Streak,
            user.DailyGoal,
            user.LastActivity,
            badges,
            weekly);
    }
}
