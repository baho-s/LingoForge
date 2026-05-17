using VocabApp.Application.Words.Dtos;

namespace VocabApp.Application.Words.Queries.GetWordList;

public sealed record GetWordListResponse(
    IReadOnlyList<WordDto> Words,
    int TotalCount);
