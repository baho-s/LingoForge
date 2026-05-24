using MediatR;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Application.Practice.Dtos;
using BeeZillion.Domain.Repositories;

namespace BeeZillion.Application.Practice.Queries.GetPracticeQuestions;

public sealed class GetPracticeQuestionsQueryHandler : IRequestHandler<GetPracticeQuestionsQuery, PracticeQuestionsResponse>
{
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUser;

    public GetPracticeQuestionsQueryHandler(
        IWordRepository wordRepository,
        ICurrentUserService currentUser)
    {
        _wordRepository = wordRepository;
        _currentUser = currentUser;
    }

    public async Task<PracticeQuestionsResponse> Handle(
        GetPracticeQuestionsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var limit = request.Limit > 0 ? request.Limit : 10;
        
        // OPTIMIZATION: GetWordsForPracticeAsync ile database'de sorting yapılır
        // limit * 2 çekilir çünkü AI Sentence filtresi memory'de yapılıyor
        var words = await _wordRepository.GetWordsForPracticeAsync(
            userId, 
            limit: limit * 2,
            cancellationToken);

        var modes = ParseModes(request.Mode);
        if (modes.Count == 0)
        {
            modes.Add("multiple_choice");
        }

        var questions = BuildQuestions(words, modes, limit);

        return new PracticeQuestionsResponse(questions);
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
        IReadOnlyList<BeeZillion.Domain.Aggregates.WordAggregate.Word> words,
        List<string> modes,
        int limit)
    {
        var questions = new List<PracticeQuestionDto>();
        if (words.Count == 0)
        {
            return questions;
        }

        var rng = new Random();
        // Smart prioritization: hiç practice'te olmayan → zor olanlar → random
        var shuffled = words
            .OrderBy(w => w.Review?.LastReviewedAt ?? DateTime.MinValue)  // En eski veya hiç (hiç = null = MinValue)
            .ThenBy(w => w.Review?.EaseFactor ?? 2.5f)  // Zor olanlar (düşük EaseFactor)
            .ThenBy(_ => Guid.NewGuid())  // Aynı düzey içinde random
            .ToList();
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
}

