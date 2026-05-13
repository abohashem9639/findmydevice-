using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindMyDevice.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceHeartbeatFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_location_enabled",
                table: "devices",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "last_battery_level",
                table: "devices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "last_is_charging",
                table: "devices",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_network_type",
                table: "devices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_location_enabled",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "last_battery_level",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "last_is_charging",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "last_network_type",
                table: "devices");
        }
    }
}
