using AsyncComponents.PubSub;
using NUnit.Framework;
using System.Threading.Tasks;

namespace AsyncComponents.Tests.PubSub
{
    [TestFixture]
    public class EventBrokerTests
    {
        private EventBroker _eventBroker;

        [SetUp]
        public void InitializeTest()
        {
            _eventBroker = new EventBroker();
        }

        [Test]
        public async Task Publish_SubscribedToSameEventType_SubscriberCalled()
        {
            var subscriber = new TestSubscriber<TestData>();

            _eventBroker
                .Subscribe<TestData>()
                .WithAction(subscriber.ExecuteAsync);

            TestData expectedData = new TestData();
            _eventBroker.Publish(expectedData);

            TestData actualData = await subscriber.WaitEventPublishedAsync();

            Assert.That(actualData, Is.EqualTo(expectedData));
        }

        [Test]
        public async Task Publish_TwoSubscribersForTheSameType_SubscribersCalled()
        {
            var subscriber1 = new TestSubscriber<TestData>();
            var subscriber2 = new TestSubscriber<TestData>();

            _eventBroker
                .Subscribe<TestData>()
                .WithAction(subscriber1.ExecuteAsync);

            _eventBroker
                .Subscribe<TestData>()
                .WithAction(subscriber2.ExecuteAsync);

            TestData expectedData = new TestData();
            _eventBroker.Publish(expectedData);

            TestData actualData1 = await subscriber1.WaitEventPublishedAsync();
            TestData actualData2 = await subscriber2.WaitEventPublishedAsync();

            Assert.That(actualData1, Is.EqualTo(expectedData));
            Assert.That(actualData2, Is.EqualTo(expectedData));
        }

        [Test]
        public async Task Publish_SubscribedToDifferentEventType_SubscriberNotCalled()
        {
            var subscriber = new TestSubscriber<OtherTestData>();

            _eventBroker
                .Subscribe<OtherTestData>()
                .WithAction(subscriber.ExecuteAsync);

            TestData expectedData = new TestData();
            await _eventBroker.Publish(expectedData).AwaitEventHandled();

            Assert.IsFalse(subscriber.Executed);
        }


        [Test]
        public async Task Publish_SubscribedToDerivedEventType_SubscriberNotCalled()
        {
            var subscriber = new TestSubscriber<TestData>();

            _eventBroker
                .Subscribe<TestData>()
                .WithAction(subscriber.ExecuteAsync);

            DerivedTestData expectedData = new DerivedTestData();
            _eventBroker.Publish(expectedData);

            TestData actualData = await subscriber.WaitEventPublishedAsync();

            Assert.That(actualData, Is.EqualTo(expectedData));
        }

        [Test]
        public async Task Publish_SubscribedToBaseEventType_SubscriberCalled()
        {
            var subscriber = new TestSubscriber<DerivedTestData>();

            _eventBroker
                .Subscribe<DerivedTestData>()
                .WithAction(subscriber.ExecuteAsync);

            TestData expectedData = new TestData();
            await _eventBroker.Publish(expectedData).AwaitEventHandled();

            Assert.IsFalse(subscriber.Executed);
        }

        [Test]
        public async Task Publish_UnsubscribedBeforePublish_SubscriberNotCalled()
        {
            var subscriber = new TestSubscriber<TestData>();

            var subscription = _eventBroker
                .Subscribe<TestData>()
                .WithAction(subscriber.ExecuteAsync);

            subscription.Dispose();

            TestData expectedData = new TestData();
            await _eventBroker.Publish(expectedData).AwaitEventHandled();

            Assert.IsFalse(subscriber.Executed);
        }
    }
}
