using MediatR;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.Users.Queries.GetStats;

public sealed class GetStatsQueryHandler : IRequestHandler<GetStatsQuery, StatsDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUser;

    public GetStatsQueryHandler(
        IUserRepository userRepository,
        IWordRepository wordRepository,
        ICurrentUserService currentUser)
    {
        _userRepository = userRepository;
        _wordRepository = wordRepository;
        _currentUser = currentUser;
    }

    public async Task<StatsDto> Handle(GetStatsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new VocabApp.Application.Common.Exceptions.NotFoundException("User not found.");
        }
        var words = await _wordRepository.GetByOwnerAsync(userId, cancellationToken);
        var now = DateTime.UtcNow;
        var oneWeekAgo = now.AddDays(-7);

        var totalWords = words.Count;
        // Son 7 gün içinde review edilen (çalışılan) kelimeler
        var wordsLearnedThisWeek = words.Count(word => 
            word.Review.LastReviewedAt.HasValue && 
            word.Review.LastReviewedAt >= oneWeekAgo);
        var averageEaseFactor = words.Any() ? words.Average(word => word.Review.EaseFactor) : 0f;

        return new StatsDto(totalWords, wordsLearnedThisWeek, averageEaseFactor);
    }
}
