using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncComponents.ProducerConsumer
{
    internal abstract class TaskQueueItem
    {
        public abstract Task ExecuteAsync(CancellationToken token);
    }

    internal class TaskQueueItem<T> : TaskQueueItem
    {
        private readonly Func<CancellationToken, Task<T>> _taskFunc;
        private readonly TaskCompletionSource<T> _taskCompletionSource;

        public TaskQueueItem(Func<CancellationToken, Task<T>> taskFunc)
        {
            _taskFunc = taskFunc;
            _taskCompletionSource = new TaskCompletionSource<T>();
        }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                T result = await _taskFunc(token);
                _taskCompletionSource.TrySetResult(result);
            }
            catch (OperationCanceledException)
            {
                _taskCompletionSource.TrySetCanceled();
            }
            catch (Exception e)
            {
                _taskCompletionSource.TrySetException(e);
            }
        }

        public async Task<T> WaitExecutionAsync(CancellationToken token)
        {
            using (token.Register(() => _taskCompletionSource.TrySetCanceled()))
            {
                return await _taskCompletionSource.Task;
            }
        }
    }
}