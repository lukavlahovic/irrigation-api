using IrrigationApi.Services;

namespace IrrigationApi.Handlers
{
    public class SensorReadingHandler : IMqttMessageHandler
    {
        public string TopicFilter => "irrigation/+/data";
        private readonly ISensorReadingService _sensorReadingService;

        public SensorReadingHandler(ISensorReadingService sensorReadingService)
        {
            _sensorReadingService = sensorReadingService;
        }

        public Task<MessageOutcome> HandleMessageAsync(string topic, string payload, CancellationToken cancellationToken)
        {
            return _sensorReadingService.ParseAndStoreAsync(topic, payload, cancellationToken);
        }
    }
}
