using VocabApp.Domain.Common;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Domain.Entities;

public sealed class UserVocabularyProgress : Entity<UserVocabularyProgressId>
{
    public UserId UserId { get; private set; }
    public WordId WordId { get; private set; }
    
    // Accuracy tracking
    public int TotalAttempts { get; private set; }
    public int CorrectAttempts { get; private set; }
    public float AccuracyRate => TotalAttempts == 0 ? 0 : (float)CorrectAttempts / TotalAttempts;
    
    // Speed tracking
    public long AverageTimeTakenMs { get; private set; }
    public long MinTimeTakenMs { get; private set; }
    public long MaxTimeTakenMs { get; private set; }
    
    // Session tracking
    public DateTime? LastSelectedAt { get; private set; }
    public int ConsecutiveSelections { get; private set; }
    
    // Metadata
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private UserVocabularyProgress() { }

    private UserVocabularyProgress(
        UserVocabularyProgressId id,
        UserId userId,
        WordId wordId)
        : base(id)
    {
        UserId = userId;
        WordId = wordId;
        TotalAttempts = 0;
        CorrectAttempts = 0;
        AverageTimeTakenMs = 0;
        MinTimeTakenMs = long.MaxValue;
        MaxTimeTakenMs = 0;
        LastSelectedAt = null;
        ConsecutiveSelections = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static UserVocabularyProgress Create(UserId userId, WordId wordId)
    {
        return new UserVocabularyProgress(
            new UserVocabularyProgressId(Guid.NewGuid()),
            userId,
            wordId);
    }

    /// <summary>
    /// Kullanıcının bir cevapını kaydeder: doğruluğu ve cevaplama süresini.
    /// AccuracyRate ve speed metrikleri güncellenir.
    /// </summary>
    public void RecordAttempt(bool isCorrect, long timeTakenMs)
    {
        TotalAttempts++;
        
        if (isCorrect)
        {
            CorrectAttempts++;
        }

        // Speed tracking
        MinTimeTakenMs = Math.Min(MinTimeTakenMs == long.MaxValue ? timeTakenMs : MinTimeTakenMs, timeTakenMs);
        MaxTimeTakenMs = Math.Max(MaxTimeTakenMs, timeTakenMs);

        // Average hesapla
        if (TotalAttempts == 1)
        {
            AverageTimeTakenMs = timeTakenMs;
        }
        else
        {
            // Yeni average = (eski total + yeni time) / yeni count
            AverageTimeTakenMs = (AverageTimeTakenMs * (TotalAttempts - 1) + timeTakenMs) / TotalAttempts;
        }

        LastSelectedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Başarılı cevap verildiğinde çağrılır.
    /// ConsecutiveSelections sayısını artırır.
    /// </summary>
    public void IncrementConsecutiveSelections()
    {
        ConsecutiveSelections++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Yanlış cevap verildiğinde çağrılır.
    /// ConsecutiveSelections sıfırlanır.
    /// </summary>
    public void ResetConsecutiveSelections()
    {
        ConsecutiveSelections = 0;
        UpdatedAt = DateTime.UtcNow;
    }
}
