using IrrigationApi.Configurations;
using IrrigationApi.Handlers;
using IrrigationApi.Routers;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace IrrigationApi.BackgroundServices
{
    public class MqttClientService : BackgroundService
    {
        private readonly ILogger<MqttClientService> _logger;
        private readonly IMqttClient _client;
        private readonly IMqttMessageRouter _mqttMessageRouter;

        private readonly MqttClientOptions _clientOptions;
        private readonly MqttClientSubscribeOptions _subOptions;

        private readonly int[] _exponentialBackoff = [1, 2, 4, 8, 16, 32];

        private int _reconnecting = 0;

        private CancellationToken _stoppingToken = default;

        public MqttClientService(IOptions<MqttSettings> options, ILogger<MqttClientService> logger, IMqttMessageRouter mqttMessageRouter)
        {
            _logger = logger;
            _mqttMessageRouter = mqttMessageRouter;

            var factory = new MqttClientFactory();
            _client = factory.CreateMqttClient();
            _clientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(options.Value.BrokerAddress, options.Value.BrokerPort)
                .WithClientId(options.Value.ClientId)
                .WithCleanSession(false)
                .Build();

            var builder = factory.CreateSubscribeOptionsBuilder();
            foreach (var topicFilter in _mqttMessageRouter.TopicFilters)
            {
                builder.WithTopicFilter(f =>
                {
                    f.WithTopic(topicFilter);
                    f.WithAtLeastOnceQoS();
                });
            }
                
            _subOptions = builder.Build();
            _client.ApplicationMessageReceivedAsync += HandleMessageAsync;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;

            _client.DisconnectedAsync += async e =>
            {
                await ConnectAndSubscribe(stoppingToken);
            };

            await ConnectAndSubscribe(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_client.IsConnected)
            {
                await _client.DisconnectAsync(cancellationToken: cancellationToken);
                _logger.LogInformation("Disconnected");
            }
            await base.StopAsync(cancellationToken);
        }

        public async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            e.AutoAcknowledge = false; // Disable auto-acknowledgment to allow for manual acknowledgment after processing

            var topic = e.ApplicationMessage.Topic;
            var shouldAcknowledge = true;

            try
            {
                var payload = e.ApplicationMessage.ConvertPayloadToString();
                _logger.LogDebug("Received on {topic}: {payload}", topic, payload);

                var outcome = await _mqttMessageRouter.RouteMessageAsync(topic, payload, _stoppingToken);

                shouldAcknowledge = outcome != MessageOutcome.TransientFailure;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while routing message for topic {topic}", topic);
                await e.AcknowledgeAsync(CancellationToken.None);
            }

            // Don't acknowledge the message if it resulted in a transient failure, allowing for re-delivery
            if (!shouldAcknowledge)
            {
                return;
            }

            try
            {
                await e.AcknowledgeAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to acknowledge message for topic {topic}", topic);
            }
        }

        public override void Dispose()
        {
            _client?.Dispose();
            base.Dispose();
        }

        private async Task ConnectAndSubscribe(CancellationToken stoppingToken)
        {
            // Prevent multiple concurrent reconnection attempts
            if (Interlocked.Exchange(ref _reconnecting, 1) != 0)
                return;

            int backoffIndex = 0;

            try
            {
                while (!_client.IsConnected && !stoppingToken.IsCancellationRequested)
                {
                    var delayIndex = Math.Min(backoffIndex, _exponentialBackoff.Length - 1);

                    try
                    {
                        var res = await _client.ConnectAsync(_clientOptions, stoppingToken);
                        _logger.LogInformation("Connected");

                        if (!res.IsSessionPresent)
                        {
                            await _client.SubscribeAsync(_subOptions, stoppingToken);
                            _logger.LogInformation("Subscribed");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (backoffIndex < _exponentialBackoff.Length || backoffIndex % 10 == 0)
                            _logger.LogWarning(ex, "Failed to reconnect after {attempts} attempts.", backoffIndex);
                        
                        await Task.Delay(TimeSpan.FromSeconds(_exponentialBackoff[delayIndex]), stoppingToken);
                        backoffIndex++;
                    }
                }                
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("App shutdown.");
            }
            finally
            {
                Interlocked.Exchange(ref _reconnecting, 0);
            }
        }
    }
}