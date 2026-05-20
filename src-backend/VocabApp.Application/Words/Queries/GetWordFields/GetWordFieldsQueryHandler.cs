using MediatR;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.Words.Queries.GetWordFields;

public sealed class GetWordFieldsQueryHandler : IRequestHandler<GetWordFieldsQuery, GetWordFieldsResponse>
{
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUser;

    public GetWordFieldsQueryHandler(IWordRepository wordRepository, ICurrentUserService currentUser)
    {
        _wordRepository = wordRepository;
        _currentUser = currentUser;
    }

    public async Task<GetWordFieldsResponse> Handle(GetWordFieldsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var fields = await _wordRepository.GetFieldCountsAsync(userId, cancellationToken);

        var mapped = fields
            .Select(field => new WordFieldCountDto(
                string.IsNullOrWhiteSpace(field.Field) ? "_no_field" : field.Field!,
                field.Count))
            .OrderBy(field => field.Field == "_no_field" ? 0 : 1)
            .ThenBy(field => field.Field)
            .ToList();

        return new GetWordFieldsResponse(mapped);
    }
}
