using MediatR;
using VocabApp.Application.Common.Exceptions;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Application.Words.Dtos;
using VocabApp.Domain.Entities;
using VocabApp.Domain.Enums;
using VocabApp.Domain.Repositories;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Application.Words.Commands.RecordReview;

public sealed class RecordReviewCommandHandler : IRequestHandler<RecordReviewCommand, WordDto>
{
    private readonly IWordRepository _wordRepository;
    private readonly IUserRepository _userRepository;
    private readonly IReviewHistoryRepository _reviewHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RecordReviewCommandHandler(
        IWordRepository wordRepository,
        IUserRepository userRepository,
        IReviewHistoryRepository reviewHistoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _wordRepository = wordRepository;
        _userRepository = userRepository;
        _reviewHistoryRepository = reviewHistoryRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<WordDto> Handle(RecordReviewCommand request, CancellationToken cancellationToken)
    {
        var wordId = new WordId(request.WordId);
        var word = await _wordRepository.GetByIdAsync(wordId, cancellationToken);
        if (word is null)
        {
            throw new NotFoundException("Word not found.");
        }

        var userId = _currentUser.GetUserId();
        if (word.OwnerId != userId)
        {
            throw new ForbiddenException("You do not have access to this word.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        word.RecordReview(request.Outcome);
        var reviewCount = user.RecordReview(DateTime.UtcNow);

        var isCorrect = request.Outcome != ReviewOutcome.Again;
        var qScore = request.Outcome switch
        {
            ReviewOutcome.Again => 0,
            ReviewOutcome.Hard => 3,
            ReviewOutcome.Good => 4,
            ReviewOutcome.Easy => 5,
            _ => (int?)null,
        };
        var reviewHistory = ReviewHistory.Create(
            userId,
            word.Id,
            isCorrect,
            request.Outcome,
            qScore,
            null,
            word.Review,
            ReviewSource.ReviewSession);
        _reviewHistoryRepository.Add(reviewHistory);

        if (reviewCount == 100)
        {
            user.AwardBadge(BadgeType.HundredReviews);
        }

        _wordRepository.Update(word);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return WordDto.FromEntity(word);
    }
}
