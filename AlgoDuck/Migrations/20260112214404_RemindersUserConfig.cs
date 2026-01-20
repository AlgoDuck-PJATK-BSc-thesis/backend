using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class RemindersUserConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "reminder_fri_hour",
                table: "user_config",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reminder_mon_hour",
                table: "user_config",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reminder_sat_hour",
                table: "user_config",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reminder_sun_hour",
                table: "user_config",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reminder_thu_hour",
                table: "user_config",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reminder_tue_hour",
                table: "user_config",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reminder_wed_hour",
                table: "user_config",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "study_reminder_next_at_utc",
                table: "user_config",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reminder_fri_hour",
                table: "user_config");

            migrationBuilder.DropColumn(
                name: "reminder_mon_hour",
                table: "user_config");

            migrationBuilder.DropColumn(
                name: "reminder_sat_hour",
                table: "user_config");

            migrationBuilder.DropColumn(
                name: "reminder_sun_hour",
                table: "user_config");

            migrationBuilder.DropColumn(
                name: "reminder_thu_hour",
                table: "user_config");

            migrationBuilder.DropColumn(
                name: "reminder_tue_hour",
                table: "user_config");

            migrationBuilder.DropColumn(
                name: "reminder_wed_hour",
                table: "user_config");

            migrationBuilder.DropColumn(
                name: "study_reminder_next_at_utc",
                table: "user_config");
        }
    }
}
