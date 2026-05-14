namespace VocabApp.Domain.ValueObjects;

public sealed record ReviewInfo(
    int IntervalDays,//Kelimenin bir sonraki incelemesinin ka� g�n sonra oldu�unu g�sterir. �nceleme sonu�lar�na g�re bu de�er g�ncellenir. Kelime ne kadar iyi biliniyorsa, bu de�er o kadar artar.
    float EaseFactor,//Kelimenin ne kadar zor oldu�unu g�sterir. 1.0'�n alt�na d��mez, 2.5'ten ba�lar, genellikle 2.5 civar�nda kal�r.
    int Repetitions,//Kelimenin ka� kez ba�ar�yla tekrarland���n� g�sterir. Her ba�ar�l� tekrar, kelimenin sonraki inceleme s�resini art�r�r.
    DateTime NextReviewAt,//Bir sonraki inceleme tarihini g�sterir. �nceleme sonu�lar�na g�re bu tarih g�ncellenir. Kelime ne kadar iyi biliniyorsa, bu tarih o kadar ileriye at�l�r.
    DateTime? LastReviewedAt//Son review tarihi. Kelimenin ne zaman incelendiğini izlemek için kullanılır.
);
