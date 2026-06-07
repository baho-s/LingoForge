using MediatR;
using BeeZillion.Application.Common.Exceptions;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Domain.Aggregates.WordAggregate;
using BeeZillion.Domain.Enums;
using BeeZillion.Domain.Repositories;

namespace BeeZillion.Application.Words.Commands.BulkCreateWords;

public sealed class BulkCreateWordsCommandHandler
    : IRequestHandler<BulkCreateWordsCommand, BulkCreateWordsResult>
{
    private readonly IWordRepository _wordRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAiSentenceService _aiSentenceService;

    public BulkCreateWordsCommandHandler(
        IWordRepository wordRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAiSentenceService aiSentenceService)
    {
        _wordRepository = wordRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _aiSentenceService = aiSentenceService;
    }

    public async Task<BulkCreateWordsResult> Handle(BulkCreateWordsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        var hasExistingWords = await _wordRepository.GetTotalCountByOwnerAsync(userId, cancellationToken) > 0;
        var createdCount = 0;
        var generatedSentenceCount = 0;

        foreach (var item in request.Items)
        {
            var word = Word.Create(userId, item.Original, item.Translation);

            if (request.GenerateSentenceImmediately)
            {
                var sentence = await _aiSentenceService.GenerateSentenceAsync(word.Original, cancellationToken);
                word.AttachAiSentence(sentence);
                generatedSentenceCount += 1;
            }

            _wordRepository.Add(word);
            createdCount += 1;
        }

        if (!hasExistingWords && createdCount > 0)
        {
            user.AwardBadge(BadgeType.FirstWord);
            _userRepository.Update(user);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BulkCreateWordsResult(createdCount, generatedSentenceCount);
    }
}