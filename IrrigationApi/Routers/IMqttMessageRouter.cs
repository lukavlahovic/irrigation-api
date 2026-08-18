using IrrigationApi.Handlers;

namespace IrrigationApi.Routers
{
    public interface IMqttMessageRouter
    {
        IEnumerable<string> TopicFilters { get; }

        Task<MessageOutcome> RouteMessageAsync(string topic, string payload, CancellationToken cancellationToken);

        void CheckTopicFilters();
    }
}