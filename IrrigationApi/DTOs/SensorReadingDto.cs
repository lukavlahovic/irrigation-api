namespace IrrigationApi.DTOs
{
    public class SensorReadingDto
    {
        public int ZoneId { get; set; }

        public decimal? Moisture { get; set; }

        public decimal? Temperature { get; set; }

        public decimal? Humidity { get; set; }

        public DateTimeOffset? RecordedAt { get; set; }
    }
}
