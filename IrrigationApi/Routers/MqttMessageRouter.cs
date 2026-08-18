using IrrigationApi.Handlers;
using MQTTnet;

namespace IrrigationApi.Routers
{
    public sealed class MqttMessageRouter : IMqttMessageRouter
    {
        private readonly IEnumerable<IMqttMessageHandler> _handlers;
        private readonly ILogger<MqttMessageRouter> _logger;
        public IEnumerable<string> TopicFilters { get; }

        public MqttMessageRouter(IEnumerable<IMqttMessageHandler> handlers, ILogger<MqttMessageRouter> logger)
        {
            _handlers = handlers.ToList();
            _logger = logger;

            TopicFilters = _handlers.Select(h => h.TopicFilter).ToList();

            CheckTopicFilters();
        }

        public async Task<MessageOutcome> RouteMessageAsync(string topic, string payload, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(topic))
            {
                _logger.LogError("Topic is null or empty. Cannot route message.");
                return MessageOutcome.PermanentFailure;
            }

            foreach (var handler in _handlers)
            {
                var isMatch = MqttTopicFilterComparer.Compare(topic, handler.TopicFilter);

                if (MqttTopicFilterCompareResult.IsMatch == isMatch)
                {
                    _logger.LogDebug("Routing message to handler for topic filter {TopicFilter}", handler.TopicFilter);
                    return await handler.HandleMessageAsync(topic, payload, cancellationToken);
                }
            }
            _logger.LogWarning("No handler found for topic {Topic}", topic);
            return MessageOutcome.PermanentFailure;
        }

        public void CheckTopicFilters()
        {
            // Validate topic filters of all handlers
            foreach (var handler in _handlers)
            {
                // using random topic to validate the filter, since we don't have a real topic to compare against
                var isValid = MqttTopicFilterComparer.Compare("test/topic", handler.TopicFilter);
                if (MqttTopicFilterCompareResult.FilterInvalid == isValid)
                {
                    throw new InvalidOperationException($"Handler {handler.GetType().Name} has an invalid topic filter: {handler.TopicFilter}");
                }
            }
        }
    }
}
