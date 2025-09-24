
using Helpi.Domain.Events;

namespace Helpi.Application.Interfaces;

public interface IEventMediator
{
    void Subscribe<TEvent, THandler>()
        where TEvent : IDomainEvent
        where THandler : IDomainEventHandler<TEvent>;

    Task Publish<TEvent>(TEvent @event) where TEvent : IDomainEvent;
}

