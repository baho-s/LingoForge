using Microsoft.AspNetCore.Mvc;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Aggregates.UserAggregate;
using VocabApp.Domain.Aggregates.WordAggregate;
using VocabApp.Domain.Repositories;

namespace VocabApp.API.Controllers;

[ApiController]
[Route("api/dev")]
public sealed class DevController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly IUserRepository _userRepository;
    private readonly IWordRepository _wordRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public DevController(
        IWebHostEnvironment environment,
        IUserRepository userRepository,
        IWordRepository wordRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _environment = environment;
        _userRepository = userRepository;
        _wordRepository = wordRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed([FromBody] SeedRequest request, CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        var user = existingUser ?? VocabApp.Domain.Aggregates.UserAggregate.User.Create(
            email,
            _passwordHasher.HashPassword(request.Password));

        if (existingUser is null)
        {
            _userRepository.Add(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var existingWords = await _wordRepository.GetByOwnerAsync(user.Id, cancellationToken);
        var existingSet = new HashSet<string>(existingWords.Select(w => w.Original), StringComparer.OrdinalIgnoreCase);

        var added = 0;
        foreach (var word in request.Words)
        {
            if (existingSet.Contains(word.Original))
            {
                continue;
            }

            var entity = Word.Create(user.Id, word.Original, word.Translation);
            _wordRepository.Add(entity);
            added += 1;
        }

        if (added > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Ok(new SeedResult(user.Id.Value, added));
    }

    public sealed record SeedRequest(string Email, string Password, IReadOnlyList<SeedWord> Words);

    public sealed record SeedWord(string Original, string Translation);

    public sealed record SeedResult(Guid UserId, int WordsAdded);
}
