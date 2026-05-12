using MediatR;

namespace VocabApp.Application.Users.Queries.GetStats;

public sealed record GetStatsQuery : IRequest<StatsDto>;
