using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class ReworkStatisticsAddRdbSolutionSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_test_case_test_case_id",
                table: "TestingResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_user_solution_solution_id",
                table: "TestingResults");

            migrationBuilder.DropForeignKey(
                name: "user_solution_status_ref",
                table: "user_solution");

            migrationBuilder.DropTable(
                name: "status");

            migrationBuilder.DropIndex(
                name: "IX_user_solution_status_id",
                table: "user_solution");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestingResults",
                table: "TestingResults");

            migrationBuilder.DropColumn(
                name: "status_id",
                table: "user_solution");

            migrationBuilder.RenameColumn(
                name: "solution_id",
                table: "TestingResults",
                newName: "UserSolutionSnapshotId");

            migrationBuilder.RenameIndex(
                name: "IX_TestingResults_solution_id",
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

            migrationBuilder.CreateTable(
                name: "code_execution_statistics",
                columns: table => new
                {
                    code_execution_statistics_id = table.Column<Guid>(type: "uuid", nullable: false),
                    problem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("code_execution_statistics_id", x => x.code_execution_statistics_id);
                    table.ForeignKey(
                        name: "FK_code_execution_statistics_application_user_user_id",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_code_execution_statistics_problem_problem_id",
                        column: x => x.problem_id,
                        principalTable: "problem",
                        principalColumn: "problem_id");
                });

            migrationBuilder.CreateTable(
                name: "user_solution_snapshots",
                columns: table => new
                {
                    user_solution_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    problem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_solution_snapshot_id", x => x.user_solution_snapshot_id);
                    table.ForeignKey(
                        name: "FK_user_solution_snapshots_application_user_user_id",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_solution_snapshots_problem_problem_id",
                        column: x => x.problem_id,
                        principalTable: "problem",
                        principalColumn: "problem_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestingResults_test_case_id",
                table: "TestingResults",
                column: "test_case_id");

            migrationBuilder.CreateIndex(
                name: "IX_code_execution_statistics_problem_id",
                table: "code_execution_statistics",
                column: "problem_id");

            migrationBuilder.CreateIndex(
                name: "IX_code_execution_statistics_user_id",
                table: "code_execution_statistics",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_solution_snapshots_problem_id",
                table: "user_solution_snapshots",
                column: "problem_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_solution_snapshots_user_id",
                table: "user_solution_snapshots",
                column: "user_id");

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
                column: "UserSolutionSnapshotId",
                principalTable: "user_solution_snapshots",
                principalColumn: "user_solution_snapshot_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_test_case_test_case_id",
                table: "TestingResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_user_solution_snapshots_UserSolutionSnapshot~",
                table: "TestingResults");

            migrationBuilder.DropTable(
                name: "code_execution_statistics");

            migrationBuilder.DropTable(
                name: "user_solution_snapshots");

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
                newName: "solution_id");

            migrationBuilder.RenameIndex(
                name: "IX_TestingResults_UserSolutionSnapshotId",
                table: "TestingResults",
                newName: "IX_TestingResults_solution_id");

            migrationBuilder.AddColumn<Guid>(
                name: "status_id",
                table: "user_solution",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestingResults",
                table: "TestingResults",
                columns: new[] { "test_case_id", "solution_id" });

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
                name: "IX_user_solution_status_id",
                table: "user_solution",
                column: "status_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestingResults_test_case_test_case_id",
                table: "TestingResults",
                column: "test_case_id",
                principalTable: "test_case",
                principalColumn: "test_case_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestingResults_user_solution_solution_id",
                table: "TestingResults",
                column: "solution_id",
                principalTable: "user_solution",
                principalColumn: "solution_id");

            migrationBuilder.AddForeignKey(
                name: "user_solution_status_ref",
                table: "user_solution",
                column: "status_id",
                principalTable: "status",
                principalColumn: "status_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
