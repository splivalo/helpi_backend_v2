// EventMediator.cs

using Helpi.Application.Interfaces;
using Helpi.Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Helpi.Infrastructure.Services;

public class EventMediator : IEventMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, List<Type>> _eventHandlers = new();

    public EventMediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IDomainEvent
        where THandler : IDomainEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        if (!_eventHandlers.ContainsKey(eventType))
            _eventHandlers[eventType] = new List<Type>();

        _eventHandlers[eventType].Add(typeof(THandler));
    }

    public async Task Publish<TEvent>(TEvent @event) where TEvent : IDomainEvent
    {

        using var scope = _serviceProvider.CreateScope(); // create a scope
        var handlers = scope.ServiceProvider
                            .GetServices<IDomainEventHandler<TEvent>>()
                            .ToList();

        foreach (var handler in handlers)
        {
            await handler.Handle(@event);
        }
        // var eventType = typeof(TEvent);
        // if (_eventHandlers.TryGetValue(eventType, out var handlers))
        // {
        //     foreach (var handlerType in handlers)
        //     {
        //         var handler = _serviceProvider.GetService(handlerType) as IDomainEventHandler<TEvent>;
        //         if (handler != null)
        //             await handler.Handle(@event);
        //     }
        // }
    }
}