using MediatR;
using VocabApp.Application.Common.Exceptions;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.Words.Commands.BulkDeleteByField;

public sealed class BulkDeleteWordsByFieldCommandHandler : IRequestHandler<BulkDeleteWordsByFieldCommand, BulkDeleteWordsByFieldResponse>
{
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public BulkDeleteWordsByFieldCommandHandler(
        IWordRepository wordRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _wordRepository = wordRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<BulkDeleteWordsByFieldResponse> Handle(
        BulkDeleteWordsByFieldCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        var field = request.Field.Trim();

        // Get all words from this field for the user
        var wordsToDelete = await _wordRepository.GetByOwnerAndFieldAsync(userId, field, cancellationToken);

        if (wordsToDelete.Count == 0)
            throw new NotFoundException($"No words found for field: {field}");

        // Delete all words
        foreach (var word in wordsToDelete)
        {
            _wordRepository.Delete(word);
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var message = $"{wordsToDelete.Count} kelime başarıyla silindi";

        return new BulkDeleteWordsByFieldResponse(
            true,
            field,
            wordsToDelete.Count,
            message);
    }
}
