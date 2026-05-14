using MediatR;
using VocabApp.Application.Practice.Dtos;

namespace VocabApp.Application.Practice.Queries.GetPracticeQuestions;

public sealed record GetPracticeQuestionsQuery(
    string? Mode = null,
    int Limit = 10) : IRequest<PracticeQuestionsResponse>;
