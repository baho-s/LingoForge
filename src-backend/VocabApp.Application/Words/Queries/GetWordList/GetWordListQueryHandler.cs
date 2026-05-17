using MediatR;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Application.Words.Dtos;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.Words.Queries.GetWordList;

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
        
        // Execute sequentially: EF Core DbContext is not thread-safe for concurrent operations
        // Even with Task.WhenAll, same DbContext instance causes concurrency issues
        var words = await _wordRepository.GetByOwnerPaginatedAsync(userId, request.Skip, request.Take, cancellationToken);
        var totalCount = await _wordRepository.GetTotalCountByOwnerAsync(userId, cancellationToken);
        
        var wordDtos = words.Select(WordDto.FromEntity).ToList();
        
        return new GetWordListResponse(wordDtos, totalCount);
    }
}
