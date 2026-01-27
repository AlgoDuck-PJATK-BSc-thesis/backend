using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeAchievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "user_achievement");

            migrationBuilder.DropColumn(
                name: "name",
                table: "user_achievement");

            migrationBuilder.DropColumn(
                name: "target_value",
                table: "user_achievement");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "user_achievement",
                newName: "achievement_code");

            migrationBuilder.CreateTable(
                name: "achievement",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    target_value = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("achievement_pk", x => x.code);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_achievement_achievement_code",
                table: "user_achievement",
                column: "achievement_code");

            migrationBuilder.AddForeignKey(
                name: "user_achievement_achievement_ref",
                table: "user_achievement",
                column: "achievement_code",
                principalTable: "achievement",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_achievement_achievement_ref",
                table: "user_achievement");

            migrationBuilder.DropTable(
                name: "achievement");

            migrationBuilder.DropIndex(
                name: "IX_user_achievement_achievement_code",
                table: "user_achievement");

            migrationBuilder.RenameColumn(
                name: "achievement_code",
                table: "user_achievement",
                newName: "code");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "user_achievement",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "user_achievement",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "target_value",
                table: "user_achievement",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
