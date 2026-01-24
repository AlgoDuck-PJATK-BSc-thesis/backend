using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class UserStudyReminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "study_reminder",
                columns: table => new
                {
                    study_reminder_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("study_reminder_pk", x => x.study_reminder_id);
                    table.CheckConstraint("study_reminder_day_of_week_chk", "day_of_week between 1 and 7");
                });

            migrationBuilder.CreateTable(
                name: "user_set_reminder",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    study_reminder_id = table.Column<int>(type: "integer", nullable: false),
                    hour = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_set_reminder_pk", x => new { x.user_id, x.study_reminder_id });
                    table.CheckConstraint("user_set_reminder_hour_chk", "hour is null or (hour between 0 and 23)");
                    table.ForeignKey(
                        name: "user_set_reminder_study_reminder_ref",
                        column: x => x.study_reminder_id,
                        principalTable: "study_reminder",
                        principalColumn: "study_reminder_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_set_reminder_user_ref",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "study_reminder",
                columns: new[] { "study_reminder_id", "code", "day_of_week", "name" },
                values: new object[,]
                {
                    { 1, "MON", 1, "Monday" },
                    { 2, "TUE", 2, "Tuesday" },
                    { 3, "WED", 3, "Wednesday" },
                    { 4, "THU", 4, "Thursday" },
                    { 5, "FRI", 5, "Friday" },
                    { 6, "SAT", 6, "Saturday" },
                    { 7, "SUN", 7, "Sunday" }
                });

            migrationBuilder.CreateIndex(
                name: "study_reminder_code_uq",
                table: "study_reminder",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "study_reminder_day_of_week_uq",
                table: "study_reminder",
                column: "day_of_week",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_set_reminder_study_reminder_id",
                table: "user_set_reminder",
                column: "study_reminder_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_set_reminder");

            migrationBuilder.DropTable(
                name: "study_reminder");

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
        }
    }
}
