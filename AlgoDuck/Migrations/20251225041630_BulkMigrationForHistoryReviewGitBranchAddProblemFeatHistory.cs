using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class BulkMigrationForHistoryReviewGitBranchAddProblemFeatHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assistance_message_assistant_chat_chat_name_problem_id_user~",
                table: "assistance_message");

            migrationBuilder.DropForeignKey(
                name: "FK_assistant_message_code_fragment_assistance_message_MessageId",
                table: "assistant_message_code_fragment");

            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_test_case_test_case_id",
                table: "TestingResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_user_solution_solution_id",
                table: "TestingResults");

            migrationBuilder.DropForeignKey(
                name: "user_solution_status_ref",
                table: "user_solution");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestingResults",
                table: "TestingResults");

            migrationBuilder.DropPrimaryKey(
                name: "assistant_chat_id",
                table: "assistant_chat");

            migrationBuilder.DropIndex(
                name: "IX_assistance_message_chat_name_problem_id_user_id",
                table: "assistance_message");

            migrationBuilder.DropColumn(
                name: "chat_name",
                table: "assistance_message");

            migrationBuilder.DropColumn(
                name: "problem_id",
                table: "assistance_message");

            migrationBuilder.RenameColumn(
                name: "status_id",
                table: "user_solution",
                newName: "StatusId");

            migrationBuilder.RenameIndex(
                name: "IX_user_solution_status_id",
                table: "user_solution",
                newName: "IX_user_solution_StatusId");

            migrationBuilder.RenameColumn(
                name: "solution_id",
                table: "TestingResults",
                newName: "UserSolutionSnapshotId");

            migrationBuilder.RenameIndex(
                name: "IX_TestingResults_solution_id",
                table: "TestingResults",
                newName: "IX_TestingResults_UserSolutionSnapshotId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "assistance_message",
                newName: "ChatId");

            migrationBuilder.AlterColumn<Guid>(
                name: "StatusId",
                table: "user_solution",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "PK_TestingResult",
                table: "TestingResults",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "assistant_chat",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestingResult",
                table: "TestingResults",
                column: "PK_TestingResult");

            migrationBuilder.AddPrimaryKey(
                name: "assistant_chat_id",
                table: "assistant_chat",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "code_execution_statistics",
                columns: table => new
                {
                    code_execution_statistics_id = table.Column<Guid>(type: "uuid", nullable: false),
                    problem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    execution_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    result = table.Column<byte>(type: "smallint", nullable: false),
                    test_case_result = table.Column<byte>(type: "smallint", nullable: false)
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
                name: "IX_assistance_message_ChatId",
                table: "assistance_message",
                column: "ChatId");

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
                name: "FK_assistance_message_assistant_chat_ChatId",
                table: "assistance_message",
                column: "ChatId",
                principalTable: "assistant_chat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_assistant_message_code_fragment_assistance_message_MessageId",
                table: "assistant_message_code_fragment",
                column: "MessageId",
                principalTable: "assistance_message",
                principalColumn: "message_id",
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
                column: "UserSolutionSnapshotId",
                principalTable: "user_solution_snapshots",
                principalColumn: "user_solution_snapshot_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_solution_status_StatusId",
                table: "user_solution",
                column: "StatusId",
                principalTable: "status",
                principalColumn: "status_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assistance_message_assistant_chat_ChatId",
                table: "assistance_message");

            migrationBuilder.DropForeignKey(
                name: "FK_assistant_message_code_fragment_assistance_message_MessageId",
                table: "assistant_message_code_fragment");

            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_test_case_test_case_id",
                table: "TestingResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TestingResults_user_solution_snapshots_UserSolutionSnapshot~",
                table: "TestingResults");

            migrationBuilder.DropForeignKey(
                name: "FK_user_solution_status_StatusId",
                table: "user_solution");

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

            migrationBuilder.DropPrimaryKey(
                name: "assistant_chat_id",
                table: "assistant_chat");

            migrationBuilder.DropIndex(
                name: "IX_assistance_message_ChatId",
                table: "assistance_message");

            migrationBuilder.DropColumn(
                name: "PK_TestingResult",
                table: "TestingResults");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "assistant_chat");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "user_solution",
                newName: "status_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_solution_StatusId",
                table: "user_solution",
                newName: "IX_user_solution_status_id");

            migrationBuilder.RenameColumn(
                name: "UserSolutionSnapshotId",
                table: "TestingResults",
                newName: "solution_id");

            migrationBuilder.RenameIndex(
                name: "IX_TestingResults_UserSolutionSnapshotId",
                table: "TestingResults",
                newName: "IX_TestingResults_solution_id");

            migrationBuilder.RenameColumn(
                name: "ChatId",
                table: "assistance_message",
                newName: "user_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "status_id",
                table: "user_solution",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "chat_name",
                table: "assistance_message",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "problem_id",
                table: "assistance_message",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestingResults",
                table: "TestingResults",
                columns: new[] { "test_case_id", "solution_id" });

            migrationBuilder.AddPrimaryKey(
                name: "assistant_chat_id",
                table: "assistant_chat",
                columns: new[] { "name", "problem_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "IX_assistance_message_chat_name_problem_id_user_id",
                table: "assistance_message",
                columns: new[] { "chat_name", "problem_id", "user_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_assistance_message_assistant_chat_chat_name_problem_id_user~",
                table: "assistance_message",
                columns: new[] { "chat_name", "problem_id", "user_id" },
                principalTable: "assistant_chat",
                principalColumns: new[] { "name", "problem_id", "user_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_assistant_message_code_fragment_assistance_message_MessageId",
                table: "assistant_message_code_fragment",
                column: "MessageId",
                principalTable: "assistance_message",
                principalColumn: "message_id");

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
