using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStatusTableAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_solution_status_StatusId",
                table: "user_solution");

            migrationBuilder.DropTable(
                name: "status");

            migrationBuilder.DropIndex(
                name: "IX_user_solution_StatusId",
                table: "user_solution");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "user_solution");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "user_solution",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "status",
                columns: table => new
                {
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("status_pk", x => x.status_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_solution_StatusId",
                table: "user_solution",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_user_solution_status_StatusId",
                table: "user_solution",
                column: "StatusId",
                principalTable: "status",
                principalColumn: "status_id");
        }
    }
}
