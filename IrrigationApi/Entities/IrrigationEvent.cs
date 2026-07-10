namespace IrrigationApi.Entities
{
    public class IrrigationEvent
    {
        public long Id { get; set; }

        public int ZoneId { get; set; }

        public Zone Zone { get; set; } = null!;

        public long? ReadingIdStart { get; set; }

        public SensorReading? ReadingStart { get; set; }

        public long? ReadingIdEnd { get; set; }

        public SensorReading? ReadingEnd { get; set; }

        public DateTimeOffset StartedAt { get; set; }

        public DateTimeOffset? StoppedAt { get; set; }
    }
}
