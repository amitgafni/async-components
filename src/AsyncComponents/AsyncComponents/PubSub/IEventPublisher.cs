namespace AsyncComponents.PubSub
{
    internal interface IEventPublisher
    {
        PublishResult Publish<T>(T eventData) where T : class;
    }
}
