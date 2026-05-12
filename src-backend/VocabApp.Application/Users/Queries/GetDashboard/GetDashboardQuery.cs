using MediatR;

namespace VocabApp.Application.Users.Queries.GetDashboard;

public sealed record GetDashboardQuery : IRequest<DashboardDto>;
