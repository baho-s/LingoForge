using MediatR;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.Words.Commands.BulkGenerate;

public sealed class BulkGenerateSentencesCommandHandler
    : IRequestHandler<BulkGenerateSentencesCommand, BulkGenerateResult>
{
    private readonly IWordRepository _wordRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAiSentenceService _aiSentenceService;

    public BulkGenerateSentencesCommandHandler(
        IWordRepository wordRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAiSentenceService aiSentenceService)
    {
        _wordRepository = wordRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _aiSentenceService = aiSentenceService;
    }

    public async Task<BulkGenerateResult> Handle(BulkGenerateSentencesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var words = await _wordRepository.GetWordsWithoutSentenceAsync(userId, cancellationToken);
        if (words.Count == 0)
        {
            return new BulkGenerateResult(0, 0);
        }

        var generated = 0;
        foreach (var word in words)
        {
            var sentence = await _aiSentenceService.GenerateSentenceAsync(word.Original, cancellationToken);
            word.AttachAiSentence(sentence);
            _wordRepository.Update(word);            
            generated += 1;
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BulkGenerateResult(generated, 0);
    }
}
