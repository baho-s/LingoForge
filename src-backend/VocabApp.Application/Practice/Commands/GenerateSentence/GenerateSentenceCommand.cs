using MediatR;
using VocabApp.Application.Practice.Dtos;

namespace VocabApp.Application.Practice.Commands.GenerateSentence;

public sealed record GenerateSentenceCommand(
    List<string> TargetVocab) : IRequest<GeneratedSentenceResponse>;
