namespace VocabApp.Application.Words.Queries.GetQuizWords;

public sealed record QuizOption(string Value, bool IsCorrect);

public sealed record QuizWordDto(
    Guid WordId,
    string Prompt,
    string Answer,
    IReadOnlyList<QuizOption> Options);
