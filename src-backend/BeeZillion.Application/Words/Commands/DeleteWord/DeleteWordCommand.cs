using MediatR;

namespace BeeZillion.Application.Words.Commands.DeleteWord;

public sealed record DeleteWordCommand(Guid WordId) : IRequest<Unit>;

