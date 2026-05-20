using MediatR;
using VocabApp.Application.Practice.Dtos;

namespace VocabApp.Application.Practice.Commands.SubmitPracticeAnswer;

public sealed record SubmitPracticeAnswerCommand(
    string QuestionId,
    string UserAnswer,
    string Type,
    string? Direction,
    long TimeTakenMs = 0) : IRequest<PracticeAnswerResponse>;
