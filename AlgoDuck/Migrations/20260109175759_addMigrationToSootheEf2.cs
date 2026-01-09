using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class addMigrationToSootheEf2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_editor_layout_application_user_UserId",
                table: "editor_layout");

            migrationBuilder.DropIndex(
                name: "IX_editor_layout_UserId",
                table: "editor_layout");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "editor_layout");

            migrationBuilder.DropColumn(
                name: "is_selected",
                table: "editor_layout");

            migrationBuilder.CreateTable(
                name: "owns_layout",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LayoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    is_selected = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_owns_layout", x => new { x.LayoutId, x.UserId });
                    table.ForeignKey(
                        name: "FK_owns_layout_application_user_UserId",
                        column: x => x.UserId,
                        principalTable: "application_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_owns_layout_editor_layout_LayoutId",
                        column: x => x.LayoutId,
                        principalTable: "editor_layout",
                        principalColumn: "editor_layout_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_owns_layout_UserId",
                table: "owns_layout",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "owns_layout");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "editor_layout",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "is_selected",
                table: "editor_layout",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_editor_layout_UserId",
                table: "editor_layout",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_editor_layout_application_user_UserId",
                table: "editor_layout",
                column: "UserId",
                principalTable: "application_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
