using MediatR;
using VocabApp.Application.Common.Events;
using VocabApp.Application.Common.Exceptions;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Events;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.Words.Commands.BulkDeleteByField;

public sealed class BulkDeleteWordsByFieldCommandHandler : IRequestHandler<BulkDeleteWordsByFieldCommand, BulkDeleteWordsByFieldResponse>
{
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public BulkDeleteWordsByFieldCommandHandler(
        IWordRepository wordRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IPublisher publisher)
    {
        _wordRepository = wordRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<BulkDeleteWordsByFieldResponse> Handle(
        BulkDeleteWordsByFieldCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        var field = request.Field == "_no_field" ? "" : request.Field.Trim();

        // Get all words from this field for the user
        var wordsToDelete = await _wordRepository.GetByOwnerAndFieldAsync(userId, field, cancellationToken);

        if (wordsToDelete.Count == 0)
            throw new NotFoundException($"No words found for field: {field}");

        // Verify ownership (security check)
        if (wordsToDelete.Any(w => w.OwnerId != userId))
            throw new ForbiddenException("You can only delete your own words.");

        // Delete all words
        foreach (var word in wordsToDelete)
        {
            _wordRepository.Delete(word);
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Dispatch domain event
        await _publisher.Publish(new DomainEventNotification<WordsDeletedFromFieldEvent>(
            new WordsDeletedFromFieldEvent(userId, request.Field, wordsToDelete.Count)), cancellationToken);

        var message = $"{wordsToDelete.Count} kelime başarıyla silindi";

        return new BulkDeleteWordsByFieldResponse(
            true,
            request.Field,
            wordsToDelete.Count,
            message);
    }
}

