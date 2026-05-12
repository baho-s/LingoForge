namespace VocabApp.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }//Bu olayýn gerçekleþtiði zamaný belirtir. Genellikle olayýn oluþtuðu zamaný kaydetmek için kullanýlýr.
}
