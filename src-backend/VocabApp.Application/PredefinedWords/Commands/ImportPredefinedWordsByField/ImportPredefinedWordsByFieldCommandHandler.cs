using MediatR;
using VocabApp.Application.Common.Exceptions;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Aggregates.WordAggregate;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.PredefinedWords.Commands.ImportPredefinedWordsByField;

public sealed class ImportPredefinedWordsByFieldCommandHandler : IRequestHandler<ImportPredefinedWordsByFieldCommand, ImportPredefinedWordsByFieldResponse>
{
    private readonly IPredefinedWordRepository _predefinedWordRepository;
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ImportPredefinedWordsByFieldCommandHandler(
        IPredefinedWordRepository predefinedWordRepository,
        IWordRepository wordRepository,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _predefinedWordRepository = predefinedWordRepository;
        _wordRepository = wordRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<ImportPredefinedWordsByFieldResponse> Handle(
        ImportPredefinedWordsByFieldCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var field = request.Field.Trim();

        // Get predefined words for this field
        var predefinedWords = await _predefinedWordRepository.GetByFieldAsync(field, cancellationToken);

        if (predefinedWords.Count == 0)
            throw new NotFoundException($"No words found for field: {field}");

        // Get user's existing words to avoid duplicates
        var userWords = await _wordRepository.GetByOwnerAsync(userId, cancellationToken);
        var userWordKeys = userWords
            .Select(w => $"{w.Original}|{w.Translation}".ToLowerInvariant())
            .ToHashSet();

        var importedCount = 0;

        foreach (var predefinedWord in predefinedWords)
        {
            var key = $"{predefinedWord.Original}|{predefinedWord.Translation}".ToLowerInvariant();

            // Skip if user already has this word
            if (userWordKeys.Contains(key))
                continue;

            // Create user's copy of the word
            var newWord = Word.Create(userId, predefinedWord.Original, predefinedWord.Translation);

            // Attach AI sentence if available
            if (!string.IsNullOrWhiteSpace(predefinedWord.AiSentence))
                newWord.AttachAiSentence(predefinedWord.AiSentence);

            _wordRepository.Add(newWord);
            importedCount++;
        }

        if (importedCount > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        var message = importedCount > 0
            ? $"{importedCount} kelime başarıyla eklendi"
            : "Zaten tüm bu kelimelere sahipsiniz";

        return new ImportPredefinedWordsByFieldResponse(
            true,
            field,
            importedCount,
            message);
    }
}
