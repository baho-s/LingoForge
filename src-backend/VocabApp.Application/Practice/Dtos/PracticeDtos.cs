using System.Text.Json.Serialization;

namespace VocabApp.Application.Practice.Dtos;

public sealed record PracticeQuestionsResponse(
    [property: JsonPropertyName("questions")] IReadOnlyList<PracticeQuestionDto> Questions);

public sealed record PracticeQuestionDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("direction")] string Direction,
    [property: JsonPropertyName("prompt")] string? Prompt,
    [property: JsonPropertyName("options")] IReadOnlyList<string>? Options,
    [property: JsonPropertyName("correct_answer")] string? CorrectAnswer,
    [property: JsonPropertyName("english_sentence")] string? EnglishSentence,
    [property: JsonPropertyName("target_words_used")] IReadOnlyList<string>? TargetWordsUsed);

public sealed record GeneratedSentenceResponse(
    [property: JsonPropertyName("sentence_id")] string SentenceId,
    [property: JsonPropertyName("english_sentence")] string EnglishSentence,
    [property: JsonPropertyName("target_words_used")] IReadOnlyList<string> TargetWordsUsed);

public sealed record PracticeAnswerResponse(
    [property: JsonPropertyName("is_correct")] bool IsCorrect,
    [property: JsonPropertyName("accuracy_score")] int? AccuracyScore,
    [property: JsonPropertyName("feedback")] string? Feedback);
