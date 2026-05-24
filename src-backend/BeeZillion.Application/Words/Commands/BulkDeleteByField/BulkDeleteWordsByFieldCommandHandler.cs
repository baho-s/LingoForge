using MediatR;
using BeeZillion.Application.Common.Events;
using BeeZillion.Application.Common.Exceptions;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Domain.Events;
using BeeZillion.Domain.Repositories;

namespace BeeZillion.Application.Words.Commands.BulkDeleteByField;

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

        try
        {
            // ✅ PERFORMANCE FIX: Bulk delete directly in database using ExecuteDeleteAsync
            // - No memory pressure on large datasets (3400+ words)
            // - No change tracking overhead
            // - Single SQL DELETE statement instead of 3400 individual operations
            var deletedCount = await _wordRepository.BulkDeleteByFieldAsync(userId, field, cancellationToken);

            if (deletedCount == 0)
                throw new NotFoundException($"No words found for field: {field}");

            // Dispatch domain event with actual deleted count
            await _publisher.Publish(new DomainEventNotification<WordsDeletedFromFieldEvent>(
                new WordsDeletedFromFieldEvent(userId, request.Field, deletedCount)), cancellationToken);

            var message = $"{deletedCount} kelime başarıyla silindi";

            return new BulkDeleteWordsByFieldResponse(
                true,
                request.Field,
                deletedCount,
                message);
        }
        catch (OperationCanceledException)
        {
            throw new ApplicationException("Silme işlemi zaman aşımına uğradı. Lütfen daha sonra tekrar deneyin.");
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Kelimeler silinirken bir hata oluştu: {ex.Message}", ex);
        }
    }
}


