using IrrigationApi.Handlers;

namespace IrrigationApi.Services
{
    public interface ISensorReadingService
    {
        Task<MessageOutcome> ParseAndStoreAsync(string topic, string payload, CancellationToken cancellationToken);
    }
}
