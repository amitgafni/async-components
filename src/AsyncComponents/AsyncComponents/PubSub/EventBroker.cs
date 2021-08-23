using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncComponents.PubSub
{
    public class EventBroker : IEventSubscriber, IEventPublisher
    {
        private readonly ConcurrentDictionary<Type, ActionSubscriber> _subscribers;

        public EventBroker()
        {
            _subscribers = new ConcurrentDictionary<Type, ActionSubscriber>();
        }

        public IActionSubscriber<T> Subscribe<T>() where T : class
        {
            return (IActionSubscriber<T>)_subscribers.GetOrAdd(typeof(T), t => new ActionSubscriber<T>());
        }

        public PublishResult Publish<T>(T eventData) where T : class
        {
            List<Task> subscribersTasks = new List<Task>();

            foreach(Type t in _subscribers.Keys)
            {
                if (t.IsAssignableFrom(typeof(T)) 
                    && _subscribers.TryGetValue(t, out ActionSubscriber actionSubscribers))
                {
                    subscribersTasks.Add(actionSubscribers.HandleEvent(eventData));
                }
            }

            return new PublishResult(Task.WhenAll(subscribersTasks));
        }
    }
}
