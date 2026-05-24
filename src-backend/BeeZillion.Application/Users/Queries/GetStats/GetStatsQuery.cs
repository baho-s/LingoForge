using MediatR;

namespace BeeZillion.Application.Users.Queries.GetStats;

public sealed record GetStatsQuery : IRequest<StatsDto>;

