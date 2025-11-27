using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTestingResultFromLinkingToProblemToLinkingToTestCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_problem_exercise_id",
                table: "TestingResults");

            migrationBuilder.RenameColumn(
                name: "exercise_id",
                table: "TestingResults",
                newName: "test_case_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestingResults_test_case_test_case_id",
                table: "TestingResults",
                column: "test_case_id",
                principalTable: "test_case",
                principalColumn: "test_case_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_test_case_test_case_id",
                table: "TestingResults");

            migrationBuilder.RenameColumn(
                name: "test_case_id",
                table: "TestingResults",
                newName: "exercise_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestingResults_problem_exercise_id",
                table: "TestingResults",
                column: "exercise_id",
                principalTable: "problem",
                principalColumn: "problem_id");
        }
    }
}
