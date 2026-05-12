using MediatR;
using VocabApp.Application.Auth.Dtos;

namespace VocabApp.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
