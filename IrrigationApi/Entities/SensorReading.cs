using Microsoft.EntityFrameworkCore;

namespace IrrigationApi.Entities
{
    public class SensorReading
    {
        public long Id { get; set; }

        public int ZoneId { get; set; }

        public Zone Zone { get; set; } = null!;

        public decimal? Moisture { get; set; }

        public decimal? Temperature { get; set; }

        public decimal? Humidity { get; set; }

        public DateTimeOffset RecordedAt { get; set; }
    }
}
