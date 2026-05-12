namespace VocabApp.Domain.ValueObjects;

public sealed record ReviewInfo(
    int IntervalDays,//Kelimenin bir sonraki incelemesinin kaç gün sonra olduðunu gösterir. Ýnceleme sonuçlarýna göre bu deðer güncellenir. Kelime ne kadar iyi biliniyorsa, bu deðer o kadar artar.
    float EaseFactor,//Kelimenin ne kadar zor olduðunu gösterir. 1.0'ün altýna düþmez, 2.5'ten baþlar, genellikle 2.5 civarýnda kalýr.
    int Repetitions,//Kelimenin kaç kez baþarýyla tekrarlandýðýný gösterir. Her baþarýlý tekrar, kelimenin sonraki inceleme süresini artýrýr.
    DateTime NextReviewAt//Bir sonraki inceleme tarihini gösterir. Ýnceleme sonuçlarýna göre bu tarih güncellenir. Kelime ne kadar iyi biliniyorsa, bu tarih o kadar ileriye atýlýr.
);
