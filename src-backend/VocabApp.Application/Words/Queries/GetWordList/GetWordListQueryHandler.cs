using MediatR;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Application.Words.Dtos;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.Words.Queries.GetWordList;

public sealed class GetWordListQueryHandler : IRequestHandler<GetWordListQuery, IReadOnlyList<WordDto>>
{
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUser;

    public GetWordListQueryHandler(IWordRepository wordRepository, ICurrentUserService currentUser)
    {
        _wordRepository = wordRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<WordDto>> Handle(GetWordListQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var words = await _wordRepository.GetByOwnerPaginatedAsync(userId, request.Skip, request.Take, cancellationToken);
        return words.Select(WordDto.FromEntity).ToList();
    }
}
