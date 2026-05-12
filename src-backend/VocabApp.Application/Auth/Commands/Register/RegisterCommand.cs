using MediatR;
using VocabApp.Application.Auth.Dtos;

namespace VocabApp.Application.Auth.Commands.Register;

public sealed record RegisterCommand(string Email, string Password) : IRequest<AuthResponse>;
