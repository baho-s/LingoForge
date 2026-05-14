using MediatR;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.PredefinedWords.Queries.GetFieldsList;

public sealed class GetFieldsListQueryHandler : IRequestHandler<GetFieldsListQuery, GetFieldsListResponse>
{
    private readonly IPredefinedWordRepository _repository;

    public GetFieldsListQueryHandler(IPredefinedWordRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetFieldsListResponse> Handle(GetFieldsListQuery request, CancellationToken cancellationToken)
    {
        var fields = await _repository.GetDistinctFieldsAsync(cancellationToken);
        return new GetFieldsListResponse(fields);
    }
}
