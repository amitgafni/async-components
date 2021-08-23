using AsyncComponents.ProducerConsumer;
using NUnit.Framework;
using System;

using System.Threading.Tasks;

namespace AsyncComponents.Tests.ProducerConsumer
{
    [TestFixture]
    public class ProducerConsumerQueueTests
    {
        private ProducerConsumerQueue _producerConsumerQueue;

        private void InitializeQueue(int consumerCount)
        {
            DisposeQueue();
            _producerConsumerQueue = new ProducerConsumerQueue(consumerCount);
        }

        [TearDown]
        public void DisposeQueue()
        {
            _producerConsumerQueue?.Dispose();
        }

        [Test]
        public void Enqueue_ShouldExecute()
        {
            const bool expectedResult = true;
            TestTaskWraper<bool> taskWraper = new TestTaskWraper<bool>();

            InitializeQueue(1);

            Task<bool> task = _producerConsumerQueue.EnqueueTask(taskWraper.TestTaskToExecute);

            taskWraper.SetResult(expectedResult);
            task.Wait();

            AssertExecution(taskWraper);
            AssertResult(expectedResult, task.Result);
        }

        [Test]
        public void Enqueue_OneConsumerTwoTasks_ExecuteFirstAndThenSecond()
        {
            const bool expectedResult = true;
            TestTaskWraper<bool> firstTaskWraper = new TestTaskWraper<bool>();
            TestTaskWraper<bool> secondTaskWraper = new TestTaskWraper<bool>();

            InitializeQueue(1);

            Task<bool> firstTask = _producerConsumerQueue.EnqueueTask(firstTaskWraper.TestTaskToExecute);
            Task<bool> secondTask = _producerConsumerQueue.EnqueueTask(secondTaskWraper.TestTaskToExecute);

            firstTaskWraper.WaitUntilExecutionStarts();
            AssertExecution(firstTaskWraper);
            AssertNoExecution(secondTaskWraper);

            firstTaskWraper.SetResult(expectedResult);

            firstTask.Wait();

            secondTaskWraper.WaitUntilExecutionStarts();
            AssertResult(expectedResult, firstTask.Result);
            AssertExecution(secondTaskWraper);

            secondTaskWraper.SetResult(expectedResult);
            secondTask.Wait();

            AssertResult(expectedResult, secondTask.Result);
        }

        [Test]
        public void Enqueue_TwoConsumerTwoTasks_ExecuteSameTime()
        {
            const bool expectedResult = true;
            TestTaskWraper<bool> firstTaskWraper = new TestTaskWraper<bool>();
            TestTaskWraper<bool> secondTaskWraper = new TestTaskWraper<bool>();

            InitializeQueue(2);

            Task<bool> firstTask = _producerConsumerQueue.EnqueueTask(firstTaskWraper.TestTaskToExecute);
            Task<bool> secondTask = _producerConsumerQueue.EnqueueTask(secondTaskWraper.TestTaskToExecute);

            firstTaskWraper.WaitUntilExecutionStarts();
            secondTaskWraper.WaitUntilExecutionStarts();

            AssertExecution(firstTaskWraper);
            AssertExecution(secondTaskWraper);

            firstTaskWraper.SetResult(expectedResult);
            secondTaskWraper.SetResult(expectedResult);

            firstTask.Wait();
            secondTask.Wait();

            AssertResult(expectedResult, firstTask.Result);
            AssertResult(expectedResult, secondTask.Result);
        }

        [Test]
        public void Enqueue_DisposeWhileRunning_TaskCanceled()
        {
            TestTaskWraper<bool> taskWraper = new TestTaskWraper<bool>();

            InitializeQueue(1);

            Task<bool> task = _producerConsumerQueue.EnqueueTask(taskWraper.TestTaskToExecute);
            taskWraper.WaitUntilExecutionStarts();

            _producerConsumerQueue.Dispose();

            AssertTaskCanceled(task);
        }

        [Test]
        public void Enqueue_DisposeBeforeRunning_TaskCanceled()
        {
            TestTaskWraper<bool> firstTaskWraper = new TestTaskWraper<bool>();
            TestTaskWraper<bool> secondTaskWraper = new TestTaskWraper<bool>();

            InitializeQueue(1);

            Task<bool> firstTask = _producerConsumerQueue.EnqueueTask(firstTaskWraper.TestTaskToExecute);
            Task<bool> secondTask = _producerConsumerQueue.EnqueueTask(secondTaskWraper.TestTaskToExecute);
            firstTaskWraper.WaitUntilExecutionStarts();

            _producerConsumerQueue.Dispose();

            AssertTaskCanceled(secondTask);
        }

        [Test]
        public void Enqueue_AfterDispose_ThrowsObjectDisposedException()
        {
            TestTaskWraper<bool> taskWraper = new TestTaskWraper<bool>();

            InitializeQueue(1);

            _producerConsumerQueue.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
                _producerConsumerQueue.EnqueueTask(taskWraper.TestTaskToExecute));
        }

        [Test]
        public void Enqueue_TaskThrows_ShouldFailTask()
        {
            TestTaskWraper<bool> taskWraper = new TestTaskWraper<bool>();

            InitializeQueue(1);

            Task<bool> task = _producerConsumerQueue.EnqueueTask(taskWraper.TestTaskToExecute);
            taskWraper.WaitUntilExecutionStarts();

            taskWraper.SetException(new Exception("Test exception"));

            AssertExecution(taskWraper);
            Assert.ThrowsAsync<Exception>(async () => await task);
        }

        [Test]
        public void Enqueue_NonResultTask_ShouldCalled()
        {
            TestTaskWraper<bool> taskWraper = new TestTaskWraper<bool>();

            InitializeQueue(1);

            Task task = _producerConsumerQueue.EnqueueTask(taskWraper.NonResultTestTaskToExecute);

            taskWraper.SetResult(true);
            task.Wait();

            AssertExecution(taskWraper);
        }

        private void AssertResult<T>(T expected, T actual)
        {
            Assert.That(expected, Is.EqualTo(actual), $"Task result expected to be:{expected} but was:{actual}");
        }

        private void AssertExecution<T>(TestTaskWraper<T> taskWraper, int callCount = 1)
        {
            Assert.That(taskWraper.CalledCount, Is.EqualTo(callCount), $"Task expected to called {callCount} times");
        }

        private void AssertNoExecution<T>(TestTaskWraper<T> taskWraper)
        {
            Assert.That(taskWraper.CalledCount, Is.EqualTo(0), "Task expected to never execute");
        }

        private void AssertTaskCanceled(Task task)
        {
            task
                .ContinueWith(t => { Assert.That(t.IsCanceled, Is.EqualTo(true), "Task was expected to be canceled"); })
                .Wait();
        }
    }
}
