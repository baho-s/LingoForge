using MediatR;
using VocabApp.Application.Words.Dtos;

namespace VocabApp.Application.Words.Queries.GetWordOfDay;

public sealed record GetWordOfDayQuery : IRequest<WordDto>;
