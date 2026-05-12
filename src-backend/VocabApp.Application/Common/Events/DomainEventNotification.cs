using MediatR;
using VocabApp.Domain.Common;

namespace VocabApp.Application.Common.Events;

public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
