using MediatR;
using VocabApp.Application.Words.Dtos;

namespace VocabApp.Application.Words.Queries.GetWordList;

public sealed record GetWordListQuery(int Skip = 0, int Take = 100, string? Field = null) : IRequest<GetWordListResponse>;
