using System;
using System.Threading.Tasks;

namespace AsyncComponents.Tests.PubSub
{
    internal class TestSubscriber<T>
    {
        private readonly TaskCompletionSource<T> _eventCalledTask;

        public TestSubscriber()
        {
            _eventCalledTask = new TaskCompletionSource<T>();
        }

        public bool Executed { get; private set; }
        public bool ThrowWhenHandled { get; set; }

        public async Task ExecuteAsync(T eventData)
        {
            _eventCalledTask.TrySetResult(eventData);
            Executed = true;
            if (ThrowWhenHandled)
            {
                throw new Exception("Event handling failed");
            }
        }

        public Task<T> WaitEventPublishedAsync() => _eventCalledTask.Task;
    }
}
