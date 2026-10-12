using MediatR;
using BeeZillion.Application.Auth.Dtos;

namespace BeeZillion.Application.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<CurrentUserResponse>;
