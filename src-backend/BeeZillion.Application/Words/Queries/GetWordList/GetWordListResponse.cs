using BeeZillion.Application.Words.Dtos;

namespace BeeZillion.Application.Words.Queries.GetWordList;

public sealed record GetWordListResponse(
    IReadOnlyList<WordDto> Words,
    int TotalCount);

