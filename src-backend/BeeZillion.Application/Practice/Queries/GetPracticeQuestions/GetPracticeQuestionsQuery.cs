using MediatR;
using BeeZillion.Application.Practice.Dtos;

namespace BeeZillion.Application.Practice.Queries.GetPracticeQuestions;

public sealed record GetPracticeQuestionsQuery(
    string? Mode = null,
    int Limit = 10) : IRequest<PracticeQuestionsResponse>;

