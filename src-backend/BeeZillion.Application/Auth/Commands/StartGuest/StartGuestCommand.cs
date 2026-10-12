using MediatR;
using BeeZillion.Application.Auth.Dtos;

namespace BeeZillion.Application.Auth.Commands.StartGuest;

public sealed record StartGuestCommand : IRequest<AuthResponse>;
