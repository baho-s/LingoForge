using MediatR;

namespace VocabApp.Application.Words.Queries.GetQuizWords;

public sealed record GetQuizWordsQuery(QuizMode Mode, int Count) : IRequest<IReadOnlyList<QuizWordDto>>;
