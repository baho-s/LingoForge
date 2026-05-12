using MediatR;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.Words.Queries.GetQuizWords;

public sealed class GetQuizWordsQueryHandler : IRequestHandler<GetQuizWordsQuery, IReadOnlyList<QuizWordDto>>
{
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUser;

    public GetQuizWordsQueryHandler(IWordRepository wordRepository, ICurrentUserService currentUser)
    {
        _wordRepository = wordRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<QuizWordDto>> Handle(GetQuizWordsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var words = await _wordRepository.GetByOwnerAsync(userId, cancellationToken);
        var pool = words
            .Where(word => word.AiSentence is not null)
            .OrderBy(_ => Guid.NewGuid())
            .Take(request.Count)
            .ToList();

        var optionsPool = words.Select(w => w.Translation).Distinct().ToList();
        var result = new List<QuizWordDto>(pool.Count);

        foreach (var word in pool)
        {
            var prompt = request.Mode == QuizMode.FillBlank
                ? (word.AiSentence ?? word.Original)
                : word.Original;

            var answer = word.Translation;
            var options = BuildOptions(answer, optionsPool, request.Mode);

            result.Add(new QuizWordDto(word.Id.Value, prompt, answer, options));
        }

        return result;
    }

    private static IReadOnlyList<QuizOption> BuildOptions(
        string answer,
        List<string> optionsPool,
        QuizMode mode)
    {
        if (mode != QuizMode.MultipleChoice)
        {
            return Array.Empty<QuizOption>();
        }

        var choices = optionsPool
            .Where(value => value != answer)
            .OrderBy(_ => Guid.NewGuid())
            .Take(3)
            .Select(value => new QuizOption(value, false))
            .ToList();

        choices.Add(new QuizOption(answer, true));
        return choices.OrderBy(_ => Guid.NewGuid()).ToList();
    }
}
