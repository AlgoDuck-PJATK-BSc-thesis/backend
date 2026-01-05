using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDescriptionFromProblemAddProblemCreationAttribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "problem");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "problem",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_problem_CreatedByUserId",
                table: "problem",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_problem_application_user_CreatedByUserId",
                table: "problem",
                column: "CreatedByUserId",
                principalTable: "application_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_problem_application_user_CreatedByUserId",
                table: "problem");

            migrationBuilder.DropIndex(
                name: "IX_problem_CreatedByUserId",
                table: "problem");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "problem");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "problem",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");
        }
    }
}
