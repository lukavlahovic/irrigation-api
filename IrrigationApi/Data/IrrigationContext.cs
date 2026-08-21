using IrrigationApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace IrrigationApi.Data
{
    public class IrrigationContext : DbContext
    {
        public IrrigationContext(DbContextOptions<IrrigationContext> options) : base(options)
        {
        }

        public DbSet<ZoneProfile> ZoneProfiles { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<SensorReading> SensorReadings { get; set; }
        public DbSet<IrrigationEvent> IrrigationEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ZoneProfile>()
                .Property(x => x.MinMoisture)
                .HasPrecision(5, 2);

            modelBuilder.Entity<ZoneProfile>()
                .Property(x => x.MaxMoisture)
                .HasPrecision(5, 2);

            modelBuilder.Entity<ZoneProfile>()
                .Property(x => x.Name)
                .HasMaxLength(100);

            modelBuilder.Entity<ZoneProfile>()
                .Property(x => x.SoilType)
                .HasMaxLength(100);

            modelBuilder.Entity<ZoneProfile>()
                .Property(x => x.CropType)
                .HasMaxLength(100);

            modelBuilder.Entity<Zone>()
                .HasOne(z => z.Profile)
                .WithMany()
                .HasForeignKey(z => z.ProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Zone>()
                .Property(x => x.Name)
                .HasMaxLength(100);

            modelBuilder.Entity<SensorReading>()
                .HasOne(sr => sr.Zone)
                .WithMany()
                .HasForeignKey(sr => sr.ZoneId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SensorReading>()
                .Property(sr => sr.Moisture)
                .HasPrecision(5, 2);

            modelBuilder.Entity<SensorReading>()
                .Property(sr => sr.Temperature)
                .HasPrecision(5, 2);

            modelBuilder.Entity<SensorReading>()
                .Property(sr => sr.Humidity)
                .HasPrecision(5, 2);

            modelBuilder.Entity<IrrigationEvent>()
                .Property(ie => ie.TriggerMoisture)
                .HasPrecision(5, 2);

            modelBuilder.Entity<IrrigationEvent>()
                .Property(ie => ie.StartTriggerReason)
                .HasConversion<string>();

            modelBuilder.Entity<IrrigationEvent>()
                .Property(ie => ie.StopTriggerReason)
                .HasConversion<string>();

            modelBuilder.Entity<IrrigationEvent>()
                .HasOne(ie => ie.Zone)
                .WithMany()
                .HasForeignKey(ie => ie.ZoneId)
                .OnDelete(DeleteBehavior.Cascade);

            // There can still be a duplicate reading if device does not send the timestamp
            modelBuilder.Entity<SensorReading>()
                .HasIndex(sr => new { sr.ZoneId, sr.RecordedAt })
                .IsUnique()
                .HasDatabaseName("ix_sensor_readings_zone_id_recorded_at");


            modelBuilder.Entity<IrrigationEvent>()
                .HasIndex(ie => new { ie.ZoneId, ie.StartedAt })
                .IsUnique()
                .HasDatabaseName("ix_irrigation_events_zone_id_started_at");
        }
    }
}
