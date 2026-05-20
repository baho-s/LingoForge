using MediatR;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.Users.Queries.GetStats;

public sealed class GetStatsQueryHandler : IRequestHandler<GetStatsQuery, StatsDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IWordRepository _wordRepository;
    private readonly IUserVocabularyProgressRepository _progressRepository;
    private readonly IReviewHistoryRepository _reviewHistoryRepository;
    private readonly ICurrentUserService _currentUser;

    public GetStatsQueryHandler(
        IUserRepository userRepository,
        IWordRepository wordRepository,
        IUserVocabularyProgressRepository progressRepository,
        IReviewHistoryRepository reviewHistoryRepository,
        ICurrentUserService currentUser)
    {
        _userRepository = userRepository;
        _wordRepository = wordRepository;
        _progressRepository = progressRepository;
        _reviewHistoryRepository = reviewHistoryRepository;
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

        // Aktivite Haritası: Son 365 gün
        var today = DateOnly.FromDateTime(now);
        var yearAgo = today.AddDays(-365);
        
        var userProgresses = await _progressRepository.GetByUserAsync(userId, cancellationToken);

        var totalAttempts = userProgresses.Sum(progress => progress.TotalAttempts);
        var correctAttempts = userProgresses.Sum(progress => progress.CorrectAttempts);
        var accuracyRate = totalAttempts > 0
            ? (float)correctAttempts / totalAttempts
            : 0f;
        var weightedTimeSum = userProgresses.Sum(progress => (double)progress.TotalAttempts * progress.AverageTimeTakenMs);
        var averageTimeTakenMs = totalAttempts > 0
            ? (long)Math.Round(weightedTimeSum / totalAttempts)
            : 0L;

        var reviewHistory = await _reviewHistoryRepository.GetByUserAsync(userId, cancellationToken);
        var correctAttemptsThisWeek = reviewHistory.Count(history => history.ReviewedAt >= oneWeekAgo && history.IsCorrect);
        
        var activityCounts = new Dictionary<DateOnly, int>();
        for (var i = 0; i < 365; i++)
        {
            activityCounts[yearAgo.AddDays(i)] = 0;
        }

        // Her progress'in UpdatedAt'sine göre o günü say (unique word sayısı olarak)
        foreach (var progress in userProgresses)
        {
            var progressDate = DateOnly.FromDateTime(progress.UpdatedAt);
            if (progressDate >= yearAgo && progressDate <= today)
            {
                if (activityCounts.ContainsKey(progressDate))
                {
                    activityCounts[progressDate]++;
                }
            }
        }

        var activityHeatmap = activityCounts
            .OrderBy(x => x.Key)
            .Select(x => new ActivityHeatmapDay(x.Key, x.Value))
            .ToList();

        return new StatsDto(
            totalWords,
            wordsLearnedThisWeek,
            averageEaseFactor,
            totalAttempts,
            correctAttempts,
            accuracyRate,
            averageTimeTakenMs,
            correctAttemptsThisWeek,
            activityHeatmap);
    }
}
