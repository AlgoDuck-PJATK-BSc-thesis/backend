using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTestCasesTOCascdeDeleteAddLastUpdatedAddProblemColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "test_case_problem",
                table: "test_case");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "problem",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "test_case_problem",
                table: "test_case",
                column: "problem_problem_id",
                principalTable: "problem",
                principalColumn: "problem_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "test_case_problem",
                table: "test_case");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "problem");

            migrationBuilder.AddForeignKey(
                name: "test_case_problem",
                table: "test_case",
                column: "problem_problem_id",
                principalTable: "problem",
                principalColumn: "problem_id");
        }
    }
}
