using MediatR;
using BeeZillion.Application.Auth.Dtos;

namespace BeeZillion.Application.Auth.Commands.Register;

public sealed record RegisterCommand(string Email, string Password) : IRequest<AuthResponse>;

