using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCodeExecutionStatisticsToSetNullUponProblemDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_code_execution_statistics_problem_problem_id",
                table: "code_execution_statistics");

            migrationBuilder.AddForeignKey(
                name: "FK_code_execution_statistics_problem_problem_id",
                table: "code_execution_statistics",
                column: "problem_id",
                principalTable: "problem",
                principalColumn: "problem_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_code_execution_statistics_problem_problem_id",
                table: "code_execution_statistics");

            migrationBuilder.AddForeignKey(
                name: "FK_code_execution_statistics_problem_problem_id",
                table: "code_execution_statistics",
                column: "problem_id",
                principalTable: "problem",
                principalColumn: "problem_id");
        }
    }
}
