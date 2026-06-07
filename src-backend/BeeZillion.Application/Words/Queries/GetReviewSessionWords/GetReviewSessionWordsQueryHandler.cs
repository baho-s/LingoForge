using MediatR;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Application.Words.Dtos;
using BeeZillion.Domain.Repositories;

namespace BeeZillion.Application.Words.Queries.GetReviewSessionWords;

public sealed class GetReviewSessionWordsQueryHandler : IRequestHandler<GetReviewSessionWordsQuery, IReadOnlyList<WordDto>>
{
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUser;

    public GetReviewSessionWordsQueryHandler(
        IWordRepository wordRepository,
        ICurrentUserService currentUser)
    {
        _wordRepository = wordRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<WordDto>> Handle(
        GetReviewSessionWordsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var limit = Math.Clamp(request.Limit, 1, 50);

        var candidateWords = request.IncludeAll
            ? await _wordRepository.GetByOwnerAsync(userId, cancellationToken)
            : await _wordRepository.GetByOwnerForReviewSessionAsync(
                userId,
                DateTime.UtcNow.Date,
                Array.Empty<Guid>(),
                limit * 2,
                cancellationToken);

        if (candidateWords.Count == 0)
        {
            return Array.Empty<WordDto>();
        }

        var orderedWords = request.IncludeAll
            ? candidateWords
                .OrderBy(word => word.Review.NextReviewAt)
                .ThenBy(word => word.Review.EaseFactor)
                .ThenByDescending(word => word.CreatedAt)
                .ToList()
            : candidateWords.ToList();

        var primaryCount = Math.Min(limit, (int)Math.Ceiling(limit * 0.5));
        var primaryWords = orderedWords.Take(primaryCount).ToList();

        var remainingWords = orderedWords
            .Skip(primaryWords.Count)
            .Take(limit - primaryWords.Count)
            .OrderBy(_ => Guid.NewGuid())
            .ToList();

        return primaryWords
            .Concat(remainingWords)
            .Select(WordDto.FromEntity)
            .ToList();
    }
}

