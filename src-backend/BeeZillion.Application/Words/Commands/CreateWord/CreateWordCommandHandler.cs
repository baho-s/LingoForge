using MediatR;
using BeeZillion.Application.Common.Exceptions;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Application.Words.Dtos;
using BeeZillion.Domain.Aggregates.WordAggregate;
using BeeZillion.Domain.Enums;
using BeeZillion.Domain.Repositories;

namespace BeeZillion.Application.Words.Commands.CreateWord;

public sealed class CreateWordCommandHandler : IRequestHandler<CreateWordCommand, WordDto>
{
    private readonly IWordRepository _wordRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAiSentenceService _aiSentenceService;

    public CreateWordCommandHandler(
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

    public async Task<WordDto> Handle(CreateWordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        var existingWords = await _wordRepository.GetByOwnerAsync(userId, cancellationToken);
        var word = Word.Create(userId, request.Original, request.Translation);
        var userUpdated = false;

        if (existingWords.Count == 0)
        {
            user.AwardBadge(BadgeType.FirstWord);
            userUpdated = true;
        }

        if (request.GenerateSentenceImmediately)
        {
            var sentence = await _aiSentenceService.GenerateSentenceAsync(word.Original, cancellationToken);
            word.AttachAiSentence(sentence);
        }

        _wordRepository.Add(word);
        if (userUpdated)
        {
            _userRepository.Update(user);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return WordDto.FromEntity(word);
    }
}

