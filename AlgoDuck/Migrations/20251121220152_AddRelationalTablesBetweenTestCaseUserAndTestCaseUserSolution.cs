using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationalTablesBetweenTestCaseUserAndTestCaseUserSolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchasedTestCase",
                columns: table => new
                {
                    test_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchasedTestCase", x => new { x.test_case_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_PurchasedTestCase_application_user_user_id",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_PurchasedTestCase_test_case_test_case_id",
                        column: x => x.test_case_id,
                        principalTable: "test_case",
                        principalColumn: "test_case_id");
                });

            migrationBuilder.CreateTable(
                name: "TestingResult",
                columns: table => new
                {
                    solution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_passed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestingResult", x => new { x.exercise_id, x.solution_id });
                    table.ForeignKey(
                        name: "FK_TestingResult_problem_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "problem",
                        principalColumn: "problem_id");
                    table.ForeignKey(
                        name: "FK_TestingResult_user_solution_solution_id",
                        column: x => x.solution_id,
                        principalTable: "user_solution",
                        principalColumn: "solution_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchasedTestCase_user_id",
                table: "PurchasedTestCase",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_TestingResult_solution_id",
                table: "TestingResult",
                column: "solution_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchasedTestCase");

            migrationBuilder.DropTable(
                name: "TestingResult");
        }
    }
}
