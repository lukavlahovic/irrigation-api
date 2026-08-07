namespace IrrigationApi.Services
{
    public interface ISensorReadingService
    {
        Task ParseAndStoreAsync(string topic, string payload, CancellationToken cancellationToken);
    }
}
