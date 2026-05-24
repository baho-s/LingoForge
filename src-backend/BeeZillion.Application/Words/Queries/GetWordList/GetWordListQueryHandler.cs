using MediatR;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Application.Words.Dtos;
using BeeZillion.Domain.Repositories;

namespace BeeZillion.Application.Words.Queries.GetWordList;

public sealed class GetWordListQueryHandler : IRequestHandler<GetWordListQuery, GetWordListResponse>
{
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUser;

    public GetWordListQueryHandler(IWordRepository wordRepository, ICurrentUserService currentUser)
    {
        _wordRepository = wordRepository;
        _currentUser = currentUser;
    }

    public async Task<GetWordListResponse> Handle(GetWordListQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        
        if (request.Field != null)
        {
            // Get paginated words for specific field
            var normalizedField = request.Field == "_no_field" ? "" : request.Field.Trim();
            var words = await _wordRepository.GetByFieldPaginatedAsync(userId, normalizedField, request.Skip, request.Take, cancellationToken);
            var totalCount = await _wordRepository.GetTotalCountByFieldAsync(userId, normalizedField, cancellationToken);
            
            var wordDtos = words.Select(WordDto.FromEntity).ToList();
            return new GetWordListResponse(wordDtos, totalCount);
        }
        
        // Get all words (original behavior)
        var allWords = await _wordRepository.GetByOwnerPaginatedAsync(userId, request.Skip, request.Take, cancellationToken);
        var allTotalCount = await _wordRepository.GetTotalCountByOwnerAsync(userId, cancellationToken);
        
        var allWordDtos = allWords.Select(WordDto.FromEntity).ToList();
        return new GetWordListResponse(allWordDtos, allTotalCount);
    }
}

