using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class MakeTestingResultsIntoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_code_execution_statistics_CodeExecutionId",
                table: "TestingResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_test_case_test_case_id",
                table: "TestingResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_user_solution_snapshots_UserSolutionSnapshot~",
                table: "TestingResults");

            migrationBuilder.RenameTable(
                name: "TestingResults",
                newName: "testing_results");

            migrationBuilder.RenameIndex(
                name: "IX_TestingResults_UserSolutionSnapshotSnapShotId",
                table: "testing_results",
                newName: "IX_testing_results_UserSolutionSnapshotSnapShotId");

            migrationBuilder.RenameIndex(
                name: "IX_TestingResults_CodeExecutionId",
                table: "testing_results",
                newName: "IX_testing_results_CodeExecutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_testing_results_code_execution_statistics_CodeExecutionId",
                table: "testing_results",
                column: "CodeExecutionId",
                principalTable: "code_execution_statistics",
                principalColumn: "code_execution_statistics_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_testing_results_test_case_test_case_id",
                table: "testing_results",
                column: "test_case_id",
                principalTable: "test_case",
                principalColumn: "test_case_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_testing_results_user_solution_snapshots_UserSolutionSnapsho~",
                table: "testing_results",
                column: "UserSolutionSnapshotSnapShotId",
                principalTable: "user_solution_snapshots",
                principalColumn: "user_solution_snapshot_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_testing_results_code_execution_statistics_CodeExecutionId",
                table: "testing_results");

            migrationBuilder.DropForeignKey(
                name: "FK_testing_results_test_case_test_case_id",
                table: "testing_results");

            migrationBuilder.DropForeignKey(
                name: "FK_testing_results_user_solution_snapshots_UserSolutionSnapsho~",
                table: "testing_results");

            migrationBuilder.RenameTable(
                name: "testing_results",
                newName: "TestingResults");

            migrationBuilder.RenameIndex(
                name: "IX_testing_results_UserSolutionSnapshotSnapShotId",
                table: "TestingResults",
                newName: "IX_TestingResults_UserSolutionSnapshotSnapShotId");

            migrationBuilder.RenameIndex(
                name: "IX_testing_results_CodeExecutionId",
                table: "TestingResults",
                newName: "IX_TestingResults_CodeExecutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestingResults_code_execution_statistics_CodeExecutionId",
                table: "TestingResults",
                column: "CodeExecutionId",
                principalTable: "code_execution_statistics",
                principalColumn: "code_execution_statistics_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TestingResults_test_case_test_case_id",
                table: "TestingResults",
                column: "test_case_id",
                principalTable: "test_case",
                principalColumn: "test_case_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TestingResults_user_solution_snapshots_UserSolutionSnapshot~",
                table: "TestingResults",
                column: "UserSolutionSnapshotSnapShotId",
                principalTable: "user_solution_snapshots",
                principalColumn: "user_solution_snapshot_id");
        }
    }
}
