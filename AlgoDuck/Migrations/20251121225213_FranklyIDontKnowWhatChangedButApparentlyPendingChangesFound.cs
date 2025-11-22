using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class FranklyIDontKnowWhatChangedButApparentlyPendingChangesFound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchasedTestCase_application_user_user_id",
                table: "PurchasedTestCase");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchasedTestCase_test_case_test_case_id",
                table: "PurchasedTestCase");

            migrationBuilder.DropForeignKey(
                name: "FK_TestingResult_problem_exercise_id",
                table: "TestingResult");

            migrationBuilder.DropForeignKey(
                name: "FK_TestingResult_user_solution_solution_id",
                table: "TestingResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestingResult",
                table: "TestingResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchasedTestCase",
                table: "PurchasedTestCase");

            migrationBuilder.RenameTable(
                name: "TestingResult",
                newName: "TestingResults");

            migrationBuilder.RenameTable(
                name: "PurchasedTestCase",
                newName: "PurchasedTestCases");

            migrationBuilder.RenameIndex(
                name: "IX_TestingResult_solution_id",
                table: "TestingResults",
                newName: "IX_TestingResults_solution_id");

            migrationBuilder.RenameIndex(
                name: "IX_PurchasedTestCase_user_id",
                table: "PurchasedTestCases",
                newName: "IX_PurchasedTestCases_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestingResults",
                table: "TestingResults",
                columns: new[] { "exercise_id", "solution_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchasedTestCases",
                table: "PurchasedTestCases",
                columns: new[] { "test_case_id", "user_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasedTestCases_application_user_user_id",
                table: "PurchasedTestCases",
                column: "user_id",
                principalTable: "application_user",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasedTestCases_test_case_test_case_id",
                table: "PurchasedTestCases",
                column: "test_case_id",
                principalTable: "test_case",
                principalColumn: "test_case_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestingResults_problem_exercise_id",
                table: "TestingResults",
                column: "exercise_id",
                principalTable: "problem",
                principalColumn: "problem_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestingResults_user_solution_solution_id",
                table: "TestingResults",
                column: "solution_id",
                principalTable: "user_solution",
                principalColumn: "solution_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchasedTestCases_application_user_user_id",
                table: "PurchasedTestCases");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchasedTestCases_test_case_test_case_id",
                table: "PurchasedTestCases");

            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_problem_exercise_id",
                table: "TestingResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_user_solution_solution_id",
                table: "TestingResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestingResults",
                table: "TestingResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchasedTestCases",
                table: "PurchasedTestCases");

            migrationBuilder.RenameTable(
                name: "TestingResults",
                newName: "TestingResult");

            migrationBuilder.RenameTable(
                name: "PurchasedTestCases",
                newName: "PurchasedTestCase");

            migrationBuilder.RenameIndex(
                name: "IX_TestingResults_solution_id",
                table: "TestingResult",
                newName: "IX_TestingResult_solution_id");

            migrationBuilder.RenameIndex(
                name: "IX_PurchasedTestCases_user_id",
                table: "PurchasedTestCase",
                newName: "IX_PurchasedTestCase_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestingResult",
                table: "TestingResult",
                columns: new[] { "exercise_id", "solution_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchasedTestCase",
                table: "PurchasedTestCase",
                columns: new[] { "test_case_id", "user_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasedTestCase_application_user_user_id",
                table: "PurchasedTestCase",
                column: "user_id",
                principalTable: "application_user",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasedTestCase_test_case_test_case_id",
                table: "PurchasedTestCase",
                column: "test_case_id",
                principalTable: "test_case",
                principalColumn: "test_case_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestingResult_problem_exercise_id",
                table: "TestingResult",
                column: "exercise_id",
                principalTable: "problem",
                principalColumn: "problem_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestingResult_user_solution_solution_id",
                table: "TestingResult",
                column: "solution_id",
                principalTable: "user_solution",
                principalColumn: "solution_id");
        }
    }
}
