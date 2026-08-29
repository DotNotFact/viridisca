using MediatR;
using Viridisca.Common.Domain;

namespace Viridisca.Common.Infrastructure.Outbox;

/// <summary>
/// Wraps a domain event as a MediatR notification. Domain events (Viridisca.Common.Domain)
/// deliberately don't reference MediatR — this adapter is what lets the interceptor publish
/// them without pulling a mediator dependency into the Domain layer.
/// </summary>
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
