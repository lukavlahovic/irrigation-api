using IrrigationApi.Configurations;
using IrrigationApi.Services;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace IrrigationApi.BackgroundServices
{
    public class MqttClientService : BackgroundService
    {
        private readonly ILogger<MqttClientService> _logger;
        private readonly IMqttClient _client;
        private readonly ISensorReadingService _sensorReadingService;

        private readonly MqttClientOptions _clientOptions;
        private readonly MqttClientSubscribeOptions _subOptions;
        private readonly string _topic;

        private readonly int[] _exponentialBackoff = [1, 2, 4, 8, 16, 32];

        private int _reconnecting = 0;

        private CancellationToken _stoppingToken = default;

        public MqttClientService(IOptions<MqttSettings> options, ILogger<MqttClientService> logger, ISensorReadingService sensorReadingService)
        {
            _logger = logger;
            _sensorReadingService = sensorReadingService;
            _topic = options.Value.Topic;

            var factory = new MqttClientFactory();
            _client = factory.CreateMqttClient();
            _clientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(options.Value.BrokerAddress, options.Value.BrokerPort)
                .WithClientId(options.Value.ClientId)
                .WithCleanSession(false)
                .Build();
            _subOptions = factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f =>
                {
                    f.WithTopic(_topic);
                    f.WithAtLeastOnceQoS();
                })
                .Build();
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
            var payload = e.ApplicationMessage.ConvertPayloadToString();
            _logger.LogDebug("Received on {topic}: {payload}", e.ApplicationMessage.Topic, payload);

            try
            {
                await _sensorReadingService.ParseAndStoreAsync(e.ApplicationMessage.Topic, payload, _stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse and store sensor reading for topic {topic}", e.ApplicationMessage.Topic);
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