using MediatR;
using BeeZillion.Application.Words.Dtos;

namespace BeeZillion.Application.Words.Queries.GetWordOfDay;

public sealed record GetWordOfDayQuery : IRequest<WordDto>;

