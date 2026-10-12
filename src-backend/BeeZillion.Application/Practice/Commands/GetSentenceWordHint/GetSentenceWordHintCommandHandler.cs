using System.Security.Cryptography;
using System.Text;
using MediatR;
using BeeZillion.Application.Common.Exceptions;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Application.Practice.Dtos;
using BeeZillion.Domain.Entities;
using BeeZillion.Domain.Repositories;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Application.Practice.Commands.GetSentenceWordHint;

public sealed class GetSentenceWordHintCommandHandler
    : IRequestHandler<GetSentenceWordHintCommand, SentenceWordHintResponse>
{
    private readonly IWordRepository _wordRepository;
    private readonly ISentenceWordHintRepository _hintRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IAiSentenceService _aiSentenceService;
    private readonly IUnitOfWork _unitOfWork;

    public GetSentenceWordHintCommandHandler(
        IWordRepository wordRepository,
        ISentenceWordHintRepository hintRepository,
        ICurrentUserService currentUser,
        IAiSentenceService aiSentenceService,
        IUnitOfWork unitOfWork)
    {
        _wordRepository = wordRepository;
        _hintRepository = hintRepository;
        _currentUser = currentUser;
        _aiSentenceService = aiSentenceService;
        _unitOfWork = unitOfWork;
    }

    public async Task<SentenceWordHintResponse> Handle(
        GetSentenceWordHintCommand request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.QuestionId, out var questionId))
            throw new ArgumentException("Invalid question id.", nameof(request.QuestionId));

        if (string.IsNullOrWhiteSpace(request.Term) || request.Term.Trim().Length > 200)
            throw new ArgumentException("A valid term is required.", nameof(request.Term));

        var word = await _wordRepository.GetByIdAsync(new WordId(questionId), cancellationToken);
        if (word is null || word.OwnerId != _currentUser.GetUserId())
            throw new NotFoundException("Practice sentence was not found.");

        if (string.IsNullOrWhiteSpace(word.AiSentence))
            throw new NotFoundException("Practice sentence was not found.");

        var term = request.Term.Trim();
        if (!word.AiSentence.Contains(term, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The requested term is not in the practice sentence.", nameof(request.Term));

        var sentenceHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(word.AiSentence)));
        var normalizedTerm = SentenceWordHint.NormalizeTerm(term);
        var savedHint = await _hintRepository.GetAsync(
            word.Id,
            sentenceHash,
            normalizedTerm,
            cancellationToken);

        if (savedHint is not null)
            return new SentenceWordHintResponse(savedHint.Term, savedHint.Translation);

        var translation = await _aiSentenceService.TranslateWordInSentenceAsync(
            word.AiSentence,
            term,
            cancellationToken);
        var hint = SentenceWordHint.Create(word.Id, sentenceHash, term, translation);
        _hintRepository.Add(hint);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SentenceWordHintResponse(hint.Term, hint.Translation);
    }
}
