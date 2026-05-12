using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Aggregates.WordAggregate;
using VocabApp.Domain.Repositories;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class PracticeController : ControllerBase
{
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IAiSentenceService _aiSentenceService;

    public PracticeController(
        IWordRepository wordRepository,
        ICurrentUserService currentUser,
        IAiSentenceService aiSentenceService)
    {
        _wordRepository = wordRepository;
        _currentUser = currentUser;
        _aiSentenceService = aiSentenceService;
    }

    [HttpGet("questions")]
    public async Task<ActionResult<PracticeQuestionsResponse>> GetQuestions(
        [FromQuery] string? mode = null,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var modes = ParseModes(mode);
        if (modes.Count == 0)
        {
            modes.Add("multiple_choice");
        }

        if (limit <= 0)
        {
            limit = 10;
        }

        var userId = _currentUser.GetUserId();
        var words = await _wordRepository.GetByOwnerAsync(userId, cancellationToken);
        var questions = BuildQuestions(words, modes, limit);

        return Ok(new PracticeQuestionsResponse(questions));
    }

    [HttpPost("generate-sentence")]
    public async Task<ActionResult<GeneratedSentenceResponse>> GenerateSentence(
        [FromBody] GenerateSentenceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.TargetVocab is null || request.TargetVocab.Count == 0)
        {
            return BadRequest("target_vocab is required.");
        }

        var prompt = string.Join(", ", request.TargetVocab);
        var sentence = await _aiSentenceService.GenerateSentenceAsync(prompt, cancellationToken);

        return Ok(new GeneratedSentenceResponse(
            Guid.NewGuid().ToString(),
            sentence,
            request.TargetVocab));
    }

    [HttpPost("submit-answer")]
    public async Task<ActionResult<PracticeAnswerResponse>> SubmitAnswer(
        [FromBody] PracticeAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserAnswer))
        {
            return Ok(new PracticeAnswerResponse(false, 0, "Answer is required."));
        }

        if (!Guid.TryParse(request.QuestionId, out var questionId))
        {
            return Ok(new PracticeAnswerResponse(false, 0, "Invalid question id."));
        }

        var word = await _wordRepository.GetByIdAsync(new WordId(questionId), cancellationToken);
        if (word is null)
        {
            return Ok(new PracticeAnswerResponse(false, 0, "Question not found."));
        }

        if (string.Equals(request.Type, "ai_sentence", StringComparison.OrdinalIgnoreCase))
        {
            var sentence = word.AiSentence ?? word.Original;
            var evaluation = await _aiSentenceService.EvaluateTranslationAsync(
                sentence,
                request.UserAnswer,
                cancellationToken);

            var isCorrect = evaluation.Score >= 70;
            return Ok(new PracticeAnswerResponse(
                isCorrect,
                evaluation.Score,
                evaluation.Feedback));
        }

        var answer = request.UserAnswer.Trim();
        var isMatch = string.Equals(answer, word.Translation, StringComparison.OrdinalIgnoreCase)
            || string.Equals(answer, word.Original, StringComparison.OrdinalIgnoreCase);

        return Ok(new PracticeAnswerResponse(
            isMatch,
            isMatch ? 100 : 0,
            isMatch ? "Correct!" : "Incorrect."));
    }

    private static List<string> ParseModes(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return new List<string>();
        }

        return mode
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => value.ToLowerInvariant())
            .Distinct()
            .ToList();
    }

    private static List<PracticeQuestionDto> BuildQuestions(
        IReadOnlyList<Word> words,
        List<string> modes,
        int limit)
    {
        var questions = new List<PracticeQuestionDto>();
        if (words.Count == 0)
        {
            return questions;
        }

        var rng = new Random();
        var shuffled = words.OrderBy(_ => Guid.NewGuid()).ToList();
        var translations = words.Select(word => word.Translation).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var originals = words.Select(word => word.Original).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var index = 0;
        while (questions.Count < limit && index < shuffled.Count)
        {
            var word = shuffled[index];
            var selectedMode = modes[questions.Count % modes.Count];

            if (selectedMode == "ai_sentence" && string.IsNullOrWhiteSpace(word.AiSentence))
            {
                index += 1;
                continue;
            }

            var direction = rng.Next(2) == 0 ? "EN_TO_TR" : "TR_TO_EN";
            if (selectedMode == "ai_sentence")
            {
                direction = "EN_TO_TR";
            }

            var prompt = direction == "EN_TO_TR" ? word.Original : word.Translation;
            var correctAnswer = direction == "EN_TO_TR" ? word.Translation : word.Original;

            switch (selectedMode)
            {
                case "multiple_choice":
                {
                    var pool = direction == "EN_TO_TR" ? translations : originals;
                    var options = BuildOptions(correctAnswer, pool);
                    questions.Add(new PracticeQuestionDto(
                        word.Id.Value.ToString(),
                        "multiple_choice",
                        direction,
                        prompt,
                        options,
                        correctAnswer,
                        null,
                        null));
                    break;
                }
                case "spelling":
                {
                    questions.Add(new PracticeQuestionDto(
                        word.Id.Value.ToString(),
                        "text_input",
                        direction,
                        prompt,
                        null,
                        correctAnswer,
                        null,
                        null));
                    break;
                }
                case "ai_sentence":
                {
                    questions.Add(new PracticeQuestionDto(
                        word.Id.Value.ToString(),
                        "ai_sentence",
                        direction,
                        null,
                        null,
                        null,
                        word.AiSentence ?? word.Original,
                        new List<string> { word.Original }));
                    break;
                }
            }

            index += 1;
        }

        return questions;
    }

    private static IReadOnlyList<string> BuildOptions(string answer, List<string> pool)
    {
        var choices = pool
            .Where(value => !string.Equals(value, answer, StringComparison.OrdinalIgnoreCase))
            .OrderBy(_ => Guid.NewGuid())
            .Take(3)
            .ToList();

        choices.Add(answer);
        return choices.OrderBy(_ => Guid.NewGuid()).ToList();
    }

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

    public sealed record GenerateSentenceRequest(
        [property: JsonPropertyName("target_vocab")] IReadOnlyList<string> TargetVocab);

    public sealed record GeneratedSentenceResponse(
        [property: JsonPropertyName("sentence_id")] string SentenceId,
        [property: JsonPropertyName("english_sentence")] string EnglishSentence,
        [property: JsonPropertyName("target_words_used")] IReadOnlyList<string> TargetWordsUsed);

    public sealed record PracticeAnswerRequest(
        [property: JsonPropertyName("question_id")] string QuestionId,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("user_answer")] string UserAnswer);

    public sealed record PracticeAnswerResponse(
        [property: JsonPropertyName("is_correct")] bool IsCorrect,
        [property: JsonPropertyName("accuracy_score")] int? AccuracyScore,
        [property: JsonPropertyName("feedback")] string? Feedback);
}
