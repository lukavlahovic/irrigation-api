namespace IrrigationApi.Handlers
{
    public interface IMqttMessageHandler
    {
        string TopicFilter { get; }

        Task<MessageOutcome> HandleMessageAsync(string topic, string payload, CancellationToken cancellationToken);
    }
}
