using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncComponents.ProducerConsumer
{
    /// <summary>
    /// The <see cref="ProducerConsumerQueue"/> provides the ability to limit the parallel execution 
    /// of asynchronos operations to a specific number of executers
    /// </summary>
    public class ProducerConsumerQueue : IDisposable
    {
        private readonly ConcurrentQueue<TaskQueueItem> _tasksQueue;
        private readonly SemaphoreSlim _consumersSemaphore;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _disposed;

        public ProducerConsumerQueue(int maxConsumersCount)
        {
            _consumersSemaphore = new SemaphoreSlim(maxConsumersCount, maxConsumersCount);
            _tasksQueue = new ConcurrentQueue<TaskQueueItem>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Number of tasks in the queue
        /// </summary>
        public int Count => _tasksQueue.Count;

        /// <summary>
        /// Enqueues an asynchronous operation which will be executed by one of the conusmers.
        /// </summary>
        /// <param name="taskFunc"></param>
        /// <returns>A Task which will complete when the operation will be executed.</returns>

        public Task EnqueueTask(Func<CancellationToken, Task> taskFunc)
        {
            Task<bool> taskWithDefaultType(CancellationToken token)
            {
                return taskFunc.Invoke(token).ContinueWith(t => true, token);
            }

            return EnqueueTask(taskWithDefaultType);
        }

        /// <summary>
        /// Enqueues an asynchronous operation which will be executed by one of the conusmers.
        /// </summary>
        /// <param name="taskFunc"></param>
        /// <returns>A Task which will complete when the operation will be executed with the returned value.</returns>
        public Task<T> EnqueueTask<T>(Func<CancellationToken, Task<T>> taskFunc)
        {
            VerifyNotDisposed();

            TaskQueueItem<T> taskQueueItem = new TaskQueueItem<T>(taskFunc);
            _tasksQueue.Enqueue(taskQueueItem);

            TriggerExecution();

            return taskQueueItem.WaitExecutionAsync(_cancellationTokenSource.Token);
        }

        private void TriggerExecution()
        {
            Task.Factory.StartNew(TriggerConsumerIfShould);
        }

        private async Task TriggerConsumerIfShould()
        {
            while (!_cancellationTokenSource.IsCancellationRequested && !_tasksQueue.IsEmpty)
            {
                if (!await _consumersSemaphore.WaitAsync(0))
                {
                    break;
                }

                try
                {
                    await ExecutePendingTasksAsync();
                }
                finally
                {
                    _consumersSemaphore.Release();
                }
            }
        }

        private async Task ExecutePendingTasksAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested
                && _tasksQueue.TryDequeue(out TaskQueueItem item))
            {
                await item.ExecuteAsync(_cancellationTokenSource.Token);
            }
        }

        private void VerifyNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ProducerConsumerQueue));
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _consumersSemaphore?.Dispose();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _disposed = true;
        }
    }
}
