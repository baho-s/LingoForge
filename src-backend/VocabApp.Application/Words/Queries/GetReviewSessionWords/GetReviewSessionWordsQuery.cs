using MediatR;
using VocabApp.Application.Words.Dtos;

namespace VocabApp.Application.Words.Queries.GetReviewSessionWords;

public sealed record GetReviewSessionWordsQuery(int Limit = 8) : IRequest<IReadOnlyList<WordDto>>;
