using MediatR;
using BeeZillion.Application.Practice.Dtos;

namespace BeeZillion.Application.Practice.Commands.GetSentenceWordHint;

public sealed record GetSentenceWordHintCommand(
    string QuestionId,
    string Term) : IRequest<SentenceWordHintResponse>;
