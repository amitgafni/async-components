namespace AsyncComponents.PubSub
{
    public interface IEventSubscriber
    {
        IActionSubscriber<T> Subscribe<T>() where T : class;
    }
}
