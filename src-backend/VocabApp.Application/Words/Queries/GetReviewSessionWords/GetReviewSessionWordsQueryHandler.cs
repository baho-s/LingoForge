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
        // ✅ IncludeAll=true ise cache'i bypass et (bütün kelimeleri tekrar göster)
        var sessionCacheKey = $"review-session:{userId.Value}:{today:yyyy-MM-dd}";
        var shownWordIdsObj = await _cacheService.GetAsync<List<Guid>>(sessionCacheKey, cancellationToken);
        var shownWordIds = request.IncludeAll ? new List<Guid>() : (shownWordIdsObj ?? new List<Guid>());

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

        // Hybrid: En gecikmişleri önce getir, kalanını çeşitlilik için karıştır.
        var primaryCount = Math.Min(limit, (int)Math.Ceiling(limit * 0.5));
        var primaryWords = todayWords.Take(primaryCount).ToList();

        var remainingNeeded = limit - primaryWords.Count;
        var remainingWords = todayWords
            .Skip(primaryWords.Count)
            .Take(remainingNeeded)
            .OrderBy(x => Guid.NewGuid())
            .ToList();

        selected.AddRange(primaryWords);
        selected.AddRange(remainingWords);

        // Bu session'da gösterilen kelimeleri cache'e kaydet (IncludeAll false ise)
        if (!request.IncludeAll)
        {
            var newShownIds = shownWordIds.Concat(selected.Select(w => w.Id.Value)).Distinct().ToList();
            var ttlUntilMidnight = today.AddDays(1) - now;
            await _cacheService.SetAsync(sessionCacheKey, newShownIds, ttlUntilMidnight, cancellationToken);
        }

        return selected.Select(WordDto.FromEntity).ToList();
    }
}
