using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

/// <summary>
/// Event publisher interface for publishing domain events.
/// Used for asynchronous event handling across services.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
}
