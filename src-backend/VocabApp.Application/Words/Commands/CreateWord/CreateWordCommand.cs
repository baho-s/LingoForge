using MediatR;
using VocabApp.Application.Words.Dtos;

namespace VocabApp.Application.Words.Commands.CreateWord;

public sealed record CreateWordCommand(
    string Original,
    string Translation,
    bool GenerateSentenceImmediately) : IRequest<WordDto>;
