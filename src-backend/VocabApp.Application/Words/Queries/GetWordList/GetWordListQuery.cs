using MediatR;
using VocabApp.Application.Words.Dtos;

namespace VocabApp.Application.Words.Queries.GetWordList;

public sealed record GetWordListQuery : IRequest<IReadOnlyList<WordDto>>;
