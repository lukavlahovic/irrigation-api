namespace IrrigationApi.Entities
{
    public enum TriggerReason
    {
        Auto = 0,
        Manual = 1
    }

    public class IrrigationEvent
    {
        public long Id { get; set; }

        public int ZoneId { get; set; }

        public Zone Zone { get; set; } = null!;

        public DateTimeOffset StartedAt { get; set; }

        public DateTimeOffset StoppedAt { get; set; }

        public decimal? TriggerMoisture { get; set; }

        public TriggerReason StartTriggerReason { get; set; }

        public TriggerReason StopTriggerReason { get; set; }

        public int ConfigVersion { get; set; }
    }
}
