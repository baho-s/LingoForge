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
        var cacheKey = $"wotd:{userId.Value}:{today}";

        var cached = await _cacheService.GetAsync<WordDto>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var word = await _wordRepository.GetWordOfTheDayAsync(userId, today, cancellationToken);
        if (word is null)
        {
            throw new NotFoundException("Word of the day not found.");
        }

        var dto = WordDto.FromEntity(word);
        var now = DateTime.UtcNow;
        var nextMidnight = now.Date.AddDays(1);
        var ttl = nextMidnight - now;
        await _cacheService.SetAsync(cacheKey, dto, ttl, cancellationToken);

        return dto;
    }
}
