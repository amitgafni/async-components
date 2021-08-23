using System.Threading.Tasks;

namespace AsyncComponents.PubSub
{
    public class PublishResult
    {
        private Task _subscribersTask;

        internal PublishResult(Task subscribersTask)
        {
            _subscribersTask = subscribersTask;
        }

        public Task AwaitEventHandled() => _subscribersTask;
    }
}
