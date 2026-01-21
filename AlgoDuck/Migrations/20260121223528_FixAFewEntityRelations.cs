using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class FixAFewEntityRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assistant_chat_application_user_user_id",
                table: "assistant_chat");

            migrationBuilder.DropForeignKey(
                name: "FK_assistant_chat_problem_problem_id",
                table: "assistant_chat");

            migrationBuilder.DropForeignKey(
                name: "FK_code_execution_statistics_application_user_user_id",
                table: "code_execution_statistics");

            migrationBuilder.DropForeignKey(
                name: "FK_code_execution_statistics_problem_problem_id",
                table: "code_execution_statistics");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchasedTestCases_application_user_user_id",
                table: "PurchasedTestCases");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchasedTestCases_test_case_test_case_id",
                table: "PurchasedTestCases");

            migrationBuilder.AddForeignKey(
                name: "FK_assistant_chat_application_user_user_id",
                table: "assistant_chat",
                column: "user_id",
                principalTable: "application_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_assistant_chat_problem_problem_id",
                table: "assistant_chat",
                column: "problem_id",
                principalTable: "problem",
                principalColumn: "problem_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_code_execution_statistics_application_user_user_id",
                table: "code_execution_statistics",
                column: "user_id",
                principalTable: "application_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_code_execution_statistics_problem_problem_id",
                table: "code_execution_statistics",
                column: "problem_id",
                principalTable: "problem",
                principalColumn: "problem_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasedTestCases_application_user_user_id",
                table: "PurchasedTestCases",
                column: "user_id",
                principalTable: "application_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasedTestCases_test_case_test_case_id",
                table: "PurchasedTestCases",
                column: "test_case_id",
                principalTable: "test_case",
                principalColumn: "test_case_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assistant_chat_application_user_user_id",
                table: "assistant_chat");

            migrationBuilder.DropForeignKey(
                name: "FK_assistant_chat_problem_problem_id",
                table: "assistant_chat");

            migrationBuilder.DropForeignKey(
                name: "FK_code_execution_statistics_application_user_user_id",
                table: "code_execution_statistics");

            migrationBuilder.DropForeignKey(
                name: "FK_code_execution_statistics_problem_problem_id",
                table: "code_execution_statistics");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchasedTestCases_application_user_user_id",
                table: "PurchasedTestCases");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchasedTestCases_test_case_test_case_id",
                table: "PurchasedTestCases");

            migrationBuilder.AddForeignKey(
                name: "FK_assistant_chat_application_user_user_id",
                table: "assistant_chat",
                column: "user_id",
                principalTable: "application_user",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_assistant_chat_problem_problem_id",
                table: "assistant_chat",
                column: "problem_id",
                principalTable: "problem",
                principalColumn: "problem_id");

            migrationBuilder.AddForeignKey(
                name: "FK_code_execution_statistics_application_user_user_id",
                table: "code_execution_statistics",
                column: "user_id",
                principalTable: "application_user",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_code_execution_statistics_problem_problem_id",
                table: "code_execution_statistics",
                column: "problem_id",
                principalTable: "problem",
                principalColumn: "problem_id",
                onDelete: ReferentialAction.SetNull);

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
        }
    }
}
