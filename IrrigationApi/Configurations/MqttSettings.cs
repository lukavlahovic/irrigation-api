namespace IrrigationApi.Configurations
{
    public class MqttSettings
    {
        public string BrokerAddress { get; set; } = string.Empty;
        public int BrokerPort { get; set; } 
        public string ClientId { get; set; } = string.Empty;
    }
}
