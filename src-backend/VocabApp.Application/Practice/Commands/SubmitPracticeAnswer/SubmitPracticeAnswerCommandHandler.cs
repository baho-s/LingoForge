using MediatR;
using VocabApp.Application.Common.Exceptions;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Application.Practice.Dtos;
using VocabApp.Domain.Entities;
using VocabApp.Domain.Enums;
using VocabApp.Domain.Repositories;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Application.Practice.Commands.SubmitPracticeAnswer;

public sealed class SubmitPracticeAnswerCommandHandler : IRequestHandler<SubmitPracticeAnswerCommand, PracticeAnswerResponse>
{
    private readonly IWordRepository _wordRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserVocabularyProgressRepository _progressRepository;
    private readonly IReviewHistoryRepository _reviewHistoryRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IAiSentenceService _aiSentenceService;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitPracticeAnswerCommandHandler(
        IWordRepository wordRepository,
        IUserRepository userRepository,
        IUserVocabularyProgressRepository progressRepository,
        IReviewHistoryRepository reviewHistoryRepository,
        ICurrentUserService currentUser,
        IAiSentenceService aiSentenceService,
        IUnitOfWork unitOfWork)
    {
        _wordRepository = wordRepository;
        _userRepository = userRepository;
        _progressRepository = progressRepository;
        _reviewHistoryRepository = reviewHistoryRepository;
        _currentUser = currentUser;
        _aiSentenceService = aiSentenceService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PracticeAnswerResponse> Handle(
        SubmitPracticeAnswerCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserAnswer))
        {
            return new PracticeAnswerResponse(false, 0, "Answer is required.");
        }

        if (!Guid.TryParse(request.QuestionId, out var questionId))
        {
            return new PracticeAnswerResponse(false, 0, "Invalid question id.");
        }

        var word = await _wordRepository.GetByIdAsync(new WordId(questionId), cancellationToken);
        if (word is null)
        {
            return new PracticeAnswerResponse(false, 0, "Question not found.");
        }

        var userId = _currentUser.GetUserId();
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        if (string.Equals(request.Type, "ai_sentence", StringComparison.OrdinalIgnoreCase))
        {
            var sentence = word.AiSentence ?? word.Original;
            var evaluation = await _aiSentenceService.EvaluateTranslationAsync(
                sentence,
                request.UserAnswer,
                cancellationToken);

            var isCorrect = evaluation.Score >= 70;
            var qScore = ReviewInfo.CalculateQScore(isCorrect, request.TimeTakenMs);
            var outcome = ReviewInfo.QScoreToReviewOutcome(qScore);
            if (isCorrect)
            {
                word.RecordReviewByQScore(true, request.TimeTakenMs);
                user.RecordReview(DateTime.UtcNow);
                
                // ✅ YENİ: UserVocabularyProgress'i kayıt et
                var progress = await _progressRepository.GetOrCreateAsync(userId, word.Id, cancellationToken);
                progress.RecordAttempt(true, request.TimeTakenMs);
                progress.IncrementConsecutiveSelections();
                // EF Core otomatik olarak takip ediyor, Update() çağrısı gerekmez
            }

            var reviewHistory = ReviewHistory.Create(
                userId,
                word.Id,
                isCorrect,
                outcome,
                qScore,
                request.TimeTakenMs,
                word.Review,
                ReviewSource.Practice);
            _reviewHistoryRepository.Add(reviewHistory);

            // Aynı şekilde Update() çağrıları gerekmez - EF Core değişiklikleri otomatik takip eder
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new PracticeAnswerResponse(
                isCorrect,
                evaluation.Score,
                evaluation.Feedback);
        }

        var answer = request.UserAnswer.Trim();
        var isMatch = string.Equals(answer, word.Translation, StringComparison.OrdinalIgnoreCase)
            || string.Equals(answer, word.Original, StringComparison.OrdinalIgnoreCase);

        // Q score mapping ile SM-2 algoritmasını uygula
        word.RecordReviewByQScore(isMatch, request.TimeTakenMs);
        user.RecordReview(DateTime.UtcNow);
        
        // ✅ YENİ: UserVocabularyProgress'i kayıt et
        var progressRecord = await _progressRepository.GetOrCreateAsync(userId, word.Id, cancellationToken);
        progressRecord.RecordAttempt(isMatch, request.TimeTakenMs);
        
        if (isMatch)
        {
            progressRecord.IncrementConsecutiveSelections();
        }
        else
        {
            progressRecord.ResetConsecutiveSelections();
        }

        var qScoreValue = ReviewInfo.CalculateQScore(isMatch, request.TimeTakenMs);
        var outcomeValue = ReviewInfo.QScoreToReviewOutcome(qScoreValue);
        var reviewHistoryRecord = ReviewHistory.Create(
            userId,
            word.Id,
            isMatch,
            outcomeValue,
            qScoreValue,
            request.TimeTakenMs,
            word.Review,
            ReviewSource.Practice);
        _reviewHistoryRepository.Add(reviewHistoryRecord);
        
        // EF Core otomatik olarak takip ediyor, Update() çağrıları gerekmez
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PracticeAnswerResponse(
            isMatch,
            isMatch ? 100 : 0,
            isMatch ? "Correct!" : "Incorrect.");
    }
}
