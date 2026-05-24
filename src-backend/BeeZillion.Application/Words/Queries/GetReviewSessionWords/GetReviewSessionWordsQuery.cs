using MediatR;
using BeeZillion.Application.Words.Dtos;

namespace BeeZillion.Application.Words.Queries.GetReviewSessionWords;

public sealed record GetReviewSessionWordsQuery(int Limit = 8, bool IncludeAll = false) : IRequest<IReadOnlyList<WordDto>>;

