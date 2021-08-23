using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncComponents.PubSub
{
    internal abstract class ActionSubscriber
    {
        internal abstract Task HandleEvent(object eventData);
    }

    internal class ActionSubscriber<T> : ActionSubscriber, IActionSubscriber<T> where T : class
    {
        private readonly ConcurrentDictionary<Guid, Func<T, Task>> _actions;

        public ActionSubscriber()
        {
            _actions = new ConcurrentDictionary<Guid, Func<T, Task>>();
        }

        internal override Task HandleEvent(object eventData)
        {
            return HandleEventAsync(eventData as T);
        }

        private Task HandleEventAsync(T eventData)
        {
            List<Task> tasks =
                _actions
                .Values
                .Select(f => SafeExecuteTask(f, eventData))
                .ToList();

            async Task SafeExecuteTask(Func<T, Task> func, T eventData)
            {
                try
                {
                    await func(eventData);
                }
                catch (Exception)
                {

                }
            }

            return Task.WhenAll(tasks);
        }



        public IDisposable WithAction(Func<T, Task> action)
        {
            Guid guid = Guid.NewGuid();

            _actions.TryAdd(guid, action);

            return new Unsubscriber<T>(_actions, guid);
        }
    }
}