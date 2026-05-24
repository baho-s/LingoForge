using MediatR;
using BeeZillion.Domain.Common;

namespace BeeZillion.Application.Common.Events;

public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;

