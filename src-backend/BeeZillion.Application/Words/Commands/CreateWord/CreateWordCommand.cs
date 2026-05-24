using MediatR;
using BeeZillion.Application.Words.Dtos;

namespace BeeZillion.Application.Words.Commands.CreateWord;

public sealed record CreateWordCommand(
    string Original,
    string Translation,
    bool GenerateSentenceImmediately) : IRequest<WordDto>;

