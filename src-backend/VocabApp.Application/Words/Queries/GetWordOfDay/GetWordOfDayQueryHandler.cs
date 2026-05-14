using MediatR;
using VocabApp.Application.Common.Exceptions;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Application.Words.Dtos;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.Words.Queries.GetWordOfDay;

public sealed class GetWordOfDayQueryHandler : IRequestHandler<GetWordOfDayQuery, WordDto>
{
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cacheService;

    public GetWordOfDayQueryHandler(
        IWordRepository wordRepository,
        ICurrentUserService currentUser,
        ICacheService cacheService)
    {
        _wordRepository = wordRepository;
        _currentUser = currentUser;
        _cacheService = cacheService;
    }

    public async Task<WordDto> Handle(GetWordOfDayQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        // 20 dakikalık cache key: 20 dakika içinde aynı kelime dönsün
        var nowMinutes = DateTime.UtcNow.Minute / 20; // 0, 1 veya 2 (3 * 20 = 60 dakika)
        var cacheKey = $"wotd:{userId.Value}:{today}:{nowMinutes}";

        var cached = await _cacheService.GetAsync<WordDto>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        // Difficulty sırasına göre kelimeleri al (Business Logic)
        var words = await _wordRepository.GetByOwnerOrderedByDifficultyAsync(userId, cancellationToken);
        if (words.Count == 0)
        {
            throw new NotFoundException("Word of the day not found.");
        }

        // En zorlanılan kelimelerin %40'ından rastgele seç (Business Logic)
        var topDifficultCount = Math.Max(1, (int)Math.Ceiling(words.Count * 0.4));
        var topDifficultWords = words.Take(topDifficultCount).ToList();

        // Deterministic random: date hash'ine göre seed (gün değişince farklı kelime)
        var seed = today.GetHashCode();
        var rng = new System.Random(seed);
        var selectedWord = topDifficultWords[rng.Next(topDifficultWords.Count)];

        var dto = WordDto.FromEntity(selectedWord);
        
        // 20 dakikalık TTL: sonraki 20 dakika checkpoint'ine kadar
        var now = DateTime.UtcNow;
        var nextCheckpoint = now.Date.AddMinutes((now.Hour * 60 + now.Minute) / 20 * 20 + 20);
        var ttl = nextCheckpoint - now;
        
        await _cacheService.SetAsync(cacheKey, dto, ttl, cancellationToken);

        return dto;
    }
}
