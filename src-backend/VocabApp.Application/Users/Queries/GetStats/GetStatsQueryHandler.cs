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

        var totalWords = words.Count;
        var withSentence = words.Count(word => word.AiSentence is not null);
        var withoutSentence = totalWords - withSentence;
        var dueToday = words.Count(word => word.Review.NextReviewAt <= now);

        return new StatsDto(totalWords, dueToday, withSentence, withoutSentence, user.ReviewCount);
    }
}
