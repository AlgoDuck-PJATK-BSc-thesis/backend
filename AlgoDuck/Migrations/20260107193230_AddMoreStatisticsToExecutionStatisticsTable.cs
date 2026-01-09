using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreStatisticsToExecutionStatisticsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_user_solution_snapshots_UserSolutionSnapshot~",
                table: "TestingResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestingResult",
                table: "TestingResults");

            migrationBuilder.DropIndex(
                name: "IX_TestingResults_test_case_id",
                table: "TestingResults");

            migrationBuilder.DropColumn(
                name: "PK_TestingResult",
                table: "TestingResults");

            migrationBuilder.RenameColumn(
                name: "UserSolutionSnapshotId",
                table: "TestingResults",
                newName: "CodeExecutionId");

            migrationBuilder.RenameIndex(
                name: "IX_TestingResults_UserSolutionSnapshotId",
                table: "TestingResults",
                newName: "IX_TestingResults_CodeExecutionId");

            migrationBuilder.AddColumn<Guid>(
                name: "UserSolutionSnapshotSnapShotId",
                table: "TestingResults",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestingResult",
                table: "TestingResults",
                columns: new[] { "test_case_id", "CodeExecutionId" });

            migrationBuilder.CreateIndex(
                name: "IX_TestingResults_UserSolutionSnapshotSnapShotId",
                table: "TestingResults",
                column: "UserSolutionSnapshotSnapShotId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestingResults_code_execution_statistics_CodeExecutionId",
                table: "TestingResults",
                column: "CodeExecutionId",
                principalTable: "code_execution_statistics",
                principalColumn: "code_execution_statistics_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TestingResults_user_solution_snapshots_UserSolutionSnapshot~",
                table: "TestingResults",
                column: "UserSolutionSnapshotSnapShotId",
                principalTable: "user_solution_snapshots",
                principalColumn: "user_solution_snapshot_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_code_execution_statistics_CodeExecutionId",
                table: "TestingResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_user_solution_snapshots_UserSolutionSnapshot~",
                table: "TestingResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestingResult",
                table: "TestingResults");

            migrationBuilder.DropIndex(
                name: "IX_TestingResults_UserSolutionSnapshotSnapShotId",
                table: "TestingResults");

            migrationBuilder.DropColumn(
                name: "UserSolutionSnapshotSnapShotId",
                table: "TestingResults");

            migrationBuilder.RenameColumn(
                name: "CodeExecutionId",
                table: "TestingResults",
                newName: "UserSolutionSnapshotId");

            migrationBuilder.RenameIndex(
                name: "IX_TestingResults_CodeExecutionId",
                table: "TestingResults",
                newName: "IX_TestingResults_UserSolutionSnapshotId");

            migrationBuilder.AddColumn<Guid>(
                name: "PK_TestingResult",
                table: "TestingResults",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestingResult",
                table: "TestingResults",
                column: "PK_TestingResult");

            migrationBuilder.CreateIndex(
                name: "IX_TestingResults_test_case_id",
                table: "TestingResults",
                column: "test_case_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestingResults_user_solution_snapshots_UserSolutionSnapshot~",
                table: "TestingResults",
                column: "UserSolutionSnapshotId",
                principalTable: "user_solution_snapshots",
                principalColumn: "user_solution_snapshot_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
