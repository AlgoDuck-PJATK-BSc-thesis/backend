using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtToUserSolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "user_solution",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AvatarKey",
                table: "user_config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "user_config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "user_achievement",
                columns: table => new
                {
                    achievement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    current_value = table.Column<int>(type: "integer", nullable: false),
                    target_value = table.Column<int>(type: "integer", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_achievement_pk", x => x.achievement_id);
                    table.ForeignKey(
                        name: "user_achievement_user_ref",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_achievement_user_id",
                table: "user_achievement",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_achievement");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "user_solution");

            migrationBuilder.DropColumn(
                name: "AvatarKey",
                table: "user_config");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "user_config");
        }
    }
}
