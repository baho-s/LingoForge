using MediatR;
using BeeZillion.Application.Practice.Dtos;

namespace BeeZillion.Application.Practice.Commands.GenerateSentence;

public sealed record GenerateSentenceCommand(
    List<string> TargetVocab) : IRequest<GeneratedSentenceResponse>;

