using MediatR;

namespace BeeZillion.Application.Users.Queries.GetDashboard;

public sealed record GetDashboardQuery : IRequest<DashboardDto>;

