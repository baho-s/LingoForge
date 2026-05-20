namespace VocabApp.Application.Words.Queries.GetWordFields;

public sealed record WordFieldCountDto(string Field, int Count);

public sealed record GetWordFieldsResponse(IReadOnlyList<WordFieldCountDto> Fields);
