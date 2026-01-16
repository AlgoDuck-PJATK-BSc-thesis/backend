using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class ReworkUserConfigAndEditoLayoutTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "editor_layout_editor_theme",
                table: "editor_layout");

            migrationBuilder.DropForeignKey(
                name: "editor_layout_user_config",
                table: "editor_layout");

            migrationBuilder.DropIndex(
                name: "IX_editor_layout_editor_theme_id",
                table: "editor_layout");

            migrationBuilder.DropColumn(
                name: "editor_theme_id",
                table: "editor_layout");

            migrationBuilder.RenameColumn(
                name: "user_config_id",
                table: "editor_layout",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_editor_layout_user_config_id",
                table: "editor_layout",
                newName: "IX_editor_layout_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "EditorThemeId",
                table: "user_config",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "editor_font_size",
                table: "user_config",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_selected",
                table: "editor_layout",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_user_config_EditorThemeId",
                table: "user_config",
                column: "EditorThemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_editor_layout_application_user_UserId",
                table: "editor_layout",
                column: "UserId",
                principalTable: "application_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_config_editor_theme_EditorThemeId",
                table: "user_config",
                column: "EditorThemeId",
                principalTable: "editor_theme",
                principalColumn: "editor_theme_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_editor_layout_application_user_UserId",
                table: "editor_layout");

            migrationBuilder.DropForeignKey(
                name: "FK_user_config_editor_theme_EditorThemeId",
                table: "user_config");

            migrationBuilder.DropIndex(
                name: "IX_user_config_EditorThemeId",
                table: "user_config");

            migrationBuilder.DropColumn(
                name: "EditorThemeId",
                table: "user_config");

            migrationBuilder.DropColumn(
                name: "editor_font_size",
                table: "user_config");

            migrationBuilder.DropColumn(
                name: "is_selected",
                table: "editor_layout");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "editor_layout",
                newName: "user_config_id");

            migrationBuilder.RenameIndex(
                name: "IX_editor_layout_UserId",
                table: "editor_layout",
                newName: "IX_editor_layout_user_config_id");

            migrationBuilder.AddColumn<Guid>(
                name: "editor_theme_id",
                table: "editor_layout",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_editor_layout_editor_theme_id",
                table: "editor_layout",
                column: "editor_theme_id");

            migrationBuilder.AddForeignKey(
                name: "editor_layout_editor_theme",
                table: "editor_layout",
                column: "editor_theme_id",
                principalTable: "editor_theme",
                principalColumn: "editor_theme_id");

            migrationBuilder.AddForeignKey(
                name: "editor_layout_user_config",
                table: "editor_layout",
                column: "user_config_id",
                principalTable: "user_config",
                principalColumn: "user_id");
        }
    }
}
