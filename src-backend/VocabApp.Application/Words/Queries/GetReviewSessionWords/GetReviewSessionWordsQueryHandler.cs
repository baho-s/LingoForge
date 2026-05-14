using MediatR;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Application.Words.Dtos;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.Words.Queries.GetReviewSessionWords;

public sealed class GetReviewSessionWordsQueryHandler : IRequestHandler<GetReviewSessionWordsQuery, IReadOnlyList<WordDto>>
{
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cacheService;

    public GetReviewSessionWordsQueryHandler(
        IWordRepository wordRepository,
        ICurrentUserService currentUser,
        ICacheService cacheService)
    {
        _wordRepository = wordRepository;
        _currentUser = currentUser;
        _cacheService = cacheService;
    }

    public async Task<IReadOnlyList<WordDto>> Handle(
        GetReviewSessionWordsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var limit = Math.Max(1, Math.Min(request.Limit, 50)); // Min 1, Max 50
        var now = DateTime.UtcNow;
        var today = now.Date;

        // Aynı gün içinde gösterilen kelimeleri cache'ten oku
        var sessionCacheKey = $"review-session:{userId.Value}:{today:yyyy-MM-dd}";
        var shownWordIdsObj = await _cacheService.GetAsync<List<Guid>>(sessionCacheKey, cancellationToken);
        var shownWordIds = shownWordIdsObj ?? new List<Guid>();

        // Tüm kelimeleri al
        var allWords = await _wordRepository.GetByOwnerAsync(userId, cancellationToken);
        if (allWords.Count == 0)
        {
            return new List<WordDto>();
        }

        // 1. TODAY's DUE kelimeleri (NextReviewAt <= today)
        var dueWords = allWords
            .Where(w => w.Review.NextReviewAt.Date <= today && !shownWordIds.Contains(w.Id.Value))
            .OrderBy(w => w.Review.NextReviewAt) // En eski sıradakiler önce
            .ToList();

        // 2. PAST günlerin kelimeleri (NextReviewAt > today, ama geçmiş)
        // Bu aslında future words, ama variety için %20-30 ekle
        var futureWords = allWords
            .Where(w => w.Review.NextReviewAt.Date > today && !shownWordIds.Contains(w.Id.Value))
            .OrderBy(w => Guid.NewGuid()) // Rastgele mix
            .ToList();

        // 3. Seçim: %70 today, %30 future
        var todayCount = (int)Math.Ceiling(limit * 0.7);
        var futureCount = limit - todayCount;

        var selected = new List<VocabApp.Domain.Aggregates.WordAggregate.Word>();

        // Today'dan seç
        selected.AddRange(dueWords.Take(todayCount));

        // Eğer today'dan yetersizse, future'dan tamamla
        var remainingNeeded = limit - selected.Count;
        if (remainingNeeded > 0)
        {
            selected.AddRange(futureWords.Take(remainingNeeded));
        }

        // Eğer hala yetersizse, daha önce gösterilen kelimeleri de ekle (bu gün başka seçenek yoksa)
        if (selected.Count < limit)
        {
            var previousShown = allWords
                .Where(w => shownWordIds.Contains(w.Id.Value) && w.Review.NextReviewAt.Date <= today.AddDays(3))
                .OrderBy(x => Guid.NewGuid())
                .Take(limit - selected.Count);
            selected.AddRange(previousShown);
        }

        // Randomize et (karışık sırada göster)
        var shuffled = selected
            .OrderBy(x => Guid.NewGuid())
            .ToList();

        // Bu session'da gösterilen kelimeleri cache'e kaydet
        var newShownIds = shownWordIds.Concat(shuffled.Select(w => w.Id.Value)).Distinct().ToList();
        var ttlUntilMidnight = today.AddDays(1) - now;
        await _cacheService.SetAsync(sessionCacheKey, newShownIds, ttlUntilMidnight, cancellationToken);

        return shuffled.Select(WordDto.FromEntity).ToList();
    }
}
