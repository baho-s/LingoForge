using MediatR;
using BeeZillion.Application.Auth.Dtos;

namespace BeeZillion.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

