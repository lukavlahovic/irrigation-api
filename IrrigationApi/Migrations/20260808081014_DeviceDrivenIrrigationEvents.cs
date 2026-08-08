using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrrigationApi.Migrations
{
    /// <inheritdoc />
    public partial class DeviceDrivenIrrigationEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_irrigation_events_sensor_readings_reading_id_end",
                table: "irrigation_events");

            migrationBuilder.DropForeignKey(
                name: "fk_irrigation_events_sensor_readings_reading_id_start",
                table: "irrigation_events");

            migrationBuilder.DropIndex(
                name: "ix_irrigation_events_reading_id_end",
                table: "irrigation_events");

            migrationBuilder.DropIndex(
                name: "ix_irrigation_events_reading_id_start",
                table: "irrigation_events");

            migrationBuilder.DropColumn(
                name: "reading_id_end",
                table: "irrigation_events");

            migrationBuilder.DropColumn(
                name: "reading_id_start",
                table: "irrigation_events");

            migrationBuilder.AddColumn<int>(
                name: "last_reported_config_version",
                table: "zones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "config_version",
                table: "zone_profiles",
                type: "integer",
                nullable: false);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "stopped_at",
                table: "irrigation_events",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "config_version",
                table: "irrigation_events",
                type: "integer",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "start_trigger_reason",
                table: "irrigation_events",
                type: "text",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "stop_trigger_reason",
                table: "irrigation_events",
                type: "text",
                nullable: false);

            migrationBuilder.AddColumn<decimal>(
                name: "trigger_moisture",
                table: "irrigation_events",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_reported_config_version",
                table: "zones");

            migrationBuilder.DropColumn(
                name: "config_version",
                table: "zone_profiles");

            migrationBuilder.DropColumn(
                name: "config_version",
                table: "irrigation_events");

            migrationBuilder.DropColumn(
                name: "start_trigger_reason",
                table: "irrigation_events");

            migrationBuilder.DropColumn(
                name: "stop_trigger_reason",
                table: "irrigation_events");

            migrationBuilder.DropColumn(
                name: "trigger_moisture",
                table: "irrigation_events");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "stopped_at",
                table: "irrigation_events",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<long>(
                name: "reading_id_end",
                table: "irrigation_events",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "reading_id_start",
                table: "irrigation_events",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_irrigation_events_reading_id_end",
                table: "irrigation_events",
                column: "reading_id_end");

            migrationBuilder.CreateIndex(
                name: "ix_irrigation_events_reading_id_start",
                table: "irrigation_events",
                column: "reading_id_start");

            migrationBuilder.AddForeignKey(
                name: "fk_irrigation_events_sensor_readings_reading_id_end",
                table: "irrigation_events",
                column: "reading_id_end",
                principalTable: "sensor_readings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_irrigation_events_sensor_readings_reading_id_start",
                table: "irrigation_events",
                column: "reading_id_start",
                principalTable: "sensor_readings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
