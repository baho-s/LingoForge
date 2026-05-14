using MediatR;
using VocabApp.Application.Common.Exceptions;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Repositories;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Application.Words.Commands.DeleteWord;

public sealed class DeleteWordCommandHandler : IRequestHandler<DeleteWordCommand, Unit>
{
    private readonly IWordRepository _wordRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWordCommandHandler(
        IWordRepository wordRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _wordRepository = wordRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteWordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        var wordId = new WordId(request.WordId);

        // Get the word
        var word = await _wordRepository.GetByIdAsync(wordId, cancellationToken);

        if (word is null)
            throw new NotFoundException($"Word with ID '{request.WordId}' not found.");

        // Verify ownership
        if (word.OwnerId != userId)
            throw new ForbiddenException("You can only delete your own words.");

        // Delete
        _wordRepository.Delete(word);

        // Save
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
