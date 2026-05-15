using VocabApp.Domain.Enums;

namespace VocabApp.Domain.ValueObjects;

public sealed record ReviewInfo(
    int IntervalDays,//Kelimenin bir sonraki incelemesinin kaç gün sonra olduğunu gösterir. İnceleme sonuçlarına göre bu değer güncellenir. Kelime ne kadar iyi biliniyorsa, bu değer o kadar artar.
    float EaseFactor,//Kelimenin ne kadar zor olduğunu gösterir. 1.0'ın altına düşmez, 2.5'ten başlar, genellikle 2.5 civarında kalır.
    int Repetitions,//Kelimenin kaç kez başarıyla tekrarlandığını gösterir. Her başarılı tekrar, kelimenin sonraki inceleme süresini artırır.
    DateTime NextReviewAt,//Bir sonraki inceleme tarihini gösterir. İnceleme sonuçlarına göre bu tarih güncellenir. Kelime ne kadar iyi biliniyorsa, bu tarih o kadar ileriye atılır.
    DateTime? LastReviewedAt//Son review tarihi. Kelimenin ne zaman incelendiğini izlemek için kullanılır.
)
{
    /// <summary>
    /// Practice sekmesinden gelen Q skoru (0-5) ve cevaplama süresini ReviewOutcome'a dönüştürür.
    /// Q = 5: Doğru + <2 saniye (çok kolay) → Easy
    /// Q = 4: Doğru + 2-6 saniye (normal) → Good
    /// Q = 3: Doğru + >6 saniye (zor ama doğru) → Hard
    /// Q = 0 veya score < 3: Yanlış → Again
    /// </summary>
    public static ReviewOutcome QScoreToReviewOutcome(int qScore)
    {
        return qScore switch
        {
            5 => ReviewOutcome.Easy,      // Doğru + hızlı
            4 => ReviewOutcome.Good,      // Doğru + normal hız
            3 => ReviewOutcome.Hard,      // Doğru + yavaş
            _ => ReviewOutcome.Again,     // Yanlış veya başarısız
        };
    }

    /// <summary>
    /// Cevap doğruluğu ve cevaplama süresinden Q skoru hesaplar (0-5).
    /// </summary>
    public static int CalculateQScore(bool isCorrect, long timeTakenMs)
    {
        if (!isCorrect)
            return 0;

        // Doğru cevap için zaman bazlı skoring
        return timeTakenMs switch
        {
            < 2000 => 5,     // < 2 saniye: mükemmel
            < 6000 => 4,     // 2-6 saniye: iyi
            _ => 3,          // > 6 saniye: zor ama doğru
        };
    }
}
