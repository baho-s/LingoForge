using MediatR;

namespace VocabApp.Application.Words.Commands.DeleteWord;

public sealed record DeleteWordCommand(Guid WordId) : IRequest<Unit>;
