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

        // ✅ N+1 QUERY FIX: Tüm kelimeleri çekmek yerine, database'de NextReviewAt <= today
        // şartıyla ve exclude edilen kelimeleri hariç tutarak, sadece ihtiyacımız olan kelimeleri çek
        // ✅ EF Core SQL Translation Fix: shownWordIds (List<Guid>) direkt WHERE IN clause'a dönüştürülür
        var dueWords = await _wordRepository.GetByOwnerForReviewSessionAsync(
            userId,
            today,
            shownWordIds,  // Already List<Guid> from cache
            limit * 2, // Buffer, future words eklenirse
            cancellationToken);

        if (dueWords.Count == 0)
        {
            return new List<WordDto>();
        }

        // 1. TODAY's DUE kelimeleri (zaten database'de filtered)
        var todayWords = dueWords.ToList();

        // 2. FUTURE words (variety için %20-30 eklemek istiyorsak)
        // Opsiyonel: Future words eklenirse, ayrı bir method çağrılabilir
        // Şimdilik focus: today's due words'ü prioritize et
        
        var selected = new List<VocabApp.Domain.Aggregates.WordAggregate.Word>();

        // Today'dan seç (%100 - tümü ihtiyaç varsa)
        var todayCount = Math.Min(limit, todayWords.Count);
        selected.AddRange(todayWords.Take(todayCount));

        // Eğer hala yetersizse, bu gün gösterilenlerin arasından tekrar seç (fallback)
        if (selected.Count < limit)
        {
            var fallbackCount = limit - selected.Count;
            var fallbackWords = todayWords
                .Skip(todayCount)
                .Take(fallbackCount);
            selected.AddRange(fallbackWords);
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
