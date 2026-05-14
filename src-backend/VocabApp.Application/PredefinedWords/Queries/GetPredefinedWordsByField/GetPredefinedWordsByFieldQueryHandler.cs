using MediatR;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.PredefinedWords.Queries.GetPredefinedWordsByField;

public sealed class GetPredefinedWordsByFieldQueryHandler : IRequestHandler<GetPredefinedWordsByFieldQuery, GetPredefinedWordsByFieldResponse>
{
    private readonly IPredefinedWordRepository _repository;

    public GetPredefinedWordsByFieldQueryHandler(IPredefinedWordRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetPredefinedWordsByFieldResponse> Handle(
        GetPredefinedWordsByFieldQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Field))
            throw new ArgumentException("Field is required.", nameof(request.Field));

        var words = await _repository.GetByFieldAsync(request.Field, cancellationToken);
        var dtos = words.Select(w => new PredefinedWordDto(
            w.Id.Value.ToString(),
            w.Field,
            w.Category,
            w.Original,
            w.Translation,
            w.AiSentence)).ToList();

        return new GetPredefinedWordsByFieldResponse(
            request.Field,
            dtos.Count,
            dtos);
    }
}
