namespace IrrigationApi.Entities
{
    public class Zone
    {
        public int Id { get; set; }

        public int ProfileId { get; set; }

        public ZoneProfile Profile { get; set; } = null!;

        public string Name { get; set; } = string.Empty;

        public bool HasMoistureSensor { get; set; }

        public bool HasTempHumiditySensor { get; set; }

        public int? LastReportedConfigVersion { get; set; }
    }
}
