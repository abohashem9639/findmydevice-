using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindMyDevice.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "device_locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    accuracy = table.Column<float>(type: "real", nullable: false),
                    altitude = table.Column<double>(type: "double precision", nullable: true),
                    speed = table.Column<float>(type: "real", nullable: true),
                    bearing = table.Column<float>(type: "real", nullable: true),
                    recorded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    received_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    battery_level = table.Column<int>(type: "integer", nullable: true),
                    is_charging = table.Column<bool>(type: "boolean", nullable: true),
                    network_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_locations", x => x.id);
                    table.ForeignKey(
                        name: "fk_device_locations_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_device_locations_device_id_recorded_at_utc",
                table: "device_locations",
                columns: new[] { "device_id", "recorded_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_locations");
        }
    }
}
