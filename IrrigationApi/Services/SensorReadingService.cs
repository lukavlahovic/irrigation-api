using Dapper;
using IrrigationApi.Data;
using Npgsql;
using System.Text.Json;

namespace IrrigationApi.Services
{
    public class SensorReadingService : ISensorReadingService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly ILogger<SensorReadingService> _logger;

        public SensorReadingService(IDbConnectionFactory connectionFactory, ILogger<SensorReadingService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task ParseAndStoreAsync(string topic, string payload, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(payload))
                {
                    _logger.LogWarning("Received empty payload for topic: {Topic}", topic);
                    return;
                }

                var sensorReading = JsonSerializer.Deserialize<DTOs.SensorReadingDto>(payload, _serializerOptions);
                
                if(sensorReading == null)
                {
                    _logger.LogWarning("Failed to deserialize payload for topic: {Topic}", topic);
                    return;
                }

                // Check if all values are null, which indicates a potential sensor malfunction
                if (sensorReading.Moisture is null && sensorReading.Temperature is null && sensorReading.Humidity is null)
                {
                    _logger.LogWarning("Sensor reading contains null values for topic: {Topic}", topic);
                    return;
                }

                // Extract the zone number from the topic string using ReadOnlySpan for efficiency
                // Expected topic format: "irrigation/zone{id}/data"
                ReadOnlySpan<char> topicSpan = topic;

                int firstSlash = topicSpan.IndexOf('/');
                if (firstSlash == -1)
                {
                    _logger.LogWarning("Invalid zone format in topic: {Topic}", topic);
                    return;
                }

                ReadOnlySpan<char> rest = topicSpan[(firstSlash + 1)..];

                int secondSlash = rest.IndexOf('/');
                if (secondSlash == -1)
                {    
                    _logger.LogWarning("Invalid zone format in topic: {Topic}", topic);
                    return;
                }

                ReadOnlySpan<char> zone = rest[..secondSlash];

                if (!zone.StartsWith("zone") ||
                    !int.TryParse(zone[4..], out int zoneNumber))
                {
                    _logger.LogWarning("Invalid zone format in topic: {Topic}", topic);
                    return;
                }

                // Validate that the ZoneId in the payload matches the zone number extracted from the topic
                if (sensorReading.ZoneId != zoneNumber)
                {
                    _logger.LogWarning("Zone ID in payload ({PayloadZoneId}) does not match zone number in topic ({TopicZoneNumber}).", sensorReading.ZoneId, zoneNumber);
                    return;
                }

                // If RecordedAt is null, set it to the current UTC time
                // This ensures that we always have a timestamp for the reading, even if the sensor did not provide one
                if (sensorReading.RecordedAt is null)
                {
                    _logger.LogDebug("RecordedAt timestamp is missing in payload for topic: {Topic}. Setting to current UTC time.", topic);
                    sensorReading.RecordedAt = DateTimeOffset.UtcNow;
                }

                await using (var conn = await _connectionFactory.CreateAsync(cancellationToken))
                {
                    var sql = """INSERT INTO sensor_readings (zone_id, moisture, temperature, humidity, recorded_at) VALUES (@ZoneId, @Moisture, @Temperature, @Humidity, @RecordedAt)""";

                    CommandDefinition command = new CommandDefinition(sql, sensorReading, cancellationToken: cancellationToken);

                    await conn.ExecuteAsync(command);
                }
            }
            catch (JsonException e)
            {
                _logger.LogError(e, "Error occurred while parsing sensor reading.");
                return;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Operation was canceled while parsing and storing sensor reading for topic: {Topic}", topic);
                return;
            }
            catch (PostgresException e)
            {
                if (e.SqlState == "23503") // Foreign key violation
                {
                    _logger.LogWarning("Invalid zone ID in sensor reading for topic: {Topic}. Error: {ErrorMessage}", topic, e.MessageText);
                }
                else
                {
                    _logger.LogError(e, "Database error occurred while storing sensor reading for topic: {Topic}", topic);
                }
                return;
            }
        }
    }
}
