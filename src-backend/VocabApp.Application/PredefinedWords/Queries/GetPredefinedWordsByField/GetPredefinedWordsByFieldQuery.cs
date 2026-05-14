using MediatR;

namespace VocabApp.Application.PredefinedWords.Queries.GetPredefinedWordsByField;

public sealed record GetPredefinedWordsByFieldQuery(string Field) : IRequest<GetPredefinedWordsByFieldResponse>;

public sealed record PredefinedWordDto(
    string Id,
    string Field,
    string? Category,
    string Original,
    string Translation,
    string? AiSentence);

public sealed record GetPredefinedWordsByFieldResponse(
    string Field,
    int TotalCount,
    IReadOnlyList<PredefinedWordDto> Words);
