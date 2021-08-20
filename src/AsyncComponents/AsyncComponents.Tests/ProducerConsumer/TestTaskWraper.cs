using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncComponents.Tests.ProducerConsumer
{
    internal class TestTaskWraper<T>
    {
        private readonly TaskCompletionSource<T> _taskCompletionSource;
        private readonly TaskCompletionSource<bool> _executionTaskSource;
        private int _calledCounter;

        public TestTaskWraper()
        {
            _taskCompletionSource = new TaskCompletionSource<T>();
            _executionTaskSource = new TaskCompletionSource<bool>();
        }

        public int CalledCount
        {
            get { return _calledCounter; }
        }

        public Task<T> TestTaskToExecute(CancellationToken token)
        {
            token.Register(SetCanceled);
            Interlocked.Increment(ref _calledCounter);
            _executionTaskSource.TrySetResult(true);
            return _taskCompletionSource.Task;
        }

        public Task NonResultTestTaskToExecute(CancellationToken token)
        {
            token.Register(SetCanceled);
            Interlocked.Increment(ref _calledCounter);
            _executionTaskSource.TrySetResult(true);
            return _taskCompletionSource.Task;
        }

        public void WaitUntilExecutionStarts()
        {
            _executionTaskSource.Task.Wait();
        }

        public void SetCanceled()
        {
            _taskCompletionSource.TrySetCanceled();
            _executionTaskSource.TrySetResult(false);
        }

        public void SetResult(T result)
        {
            _taskCompletionSource.TrySetResult(result);
        }

        public void SetException(Exception ex)
        {
            _taskCompletionSource.TrySetException(ex);
            _executionTaskSource.TrySetResult(false);
        }
    }
}