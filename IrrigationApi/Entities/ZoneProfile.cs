using Microsoft.EntityFrameworkCore;

namespace IrrigationApi.Entities
{
    public class ZoneProfile
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? SoilType { get; set; }

        public string? CropType { get; set; }
        
        public decimal MinMoisture { get; set; }

        public decimal MaxMoisture { get; set; }
    }
}
