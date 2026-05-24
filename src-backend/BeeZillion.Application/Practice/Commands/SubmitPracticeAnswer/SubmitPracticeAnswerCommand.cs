using MediatR;
using BeeZillion.Application.Practice.Dtos;

namespace BeeZillion.Application.Practice.Commands.SubmitPracticeAnswer;

public sealed record SubmitPracticeAnswerCommand(
    string QuestionId,
    string UserAnswer,
    string Type,
    string? Direction,
    long TimeTakenMs = 0) : IRequest<PracticeAnswerResponse>;

