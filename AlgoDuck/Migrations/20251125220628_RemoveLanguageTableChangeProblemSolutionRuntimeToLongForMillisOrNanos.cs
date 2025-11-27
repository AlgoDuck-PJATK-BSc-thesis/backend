using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLanguageTableChangeProblemSolutionRuntimeToLongForMillisOrNanos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_solution_language_ref",
                table: "user_solution");

            migrationBuilder.DropTable(
                name: "language");

            migrationBuilder.DropIndex(
                name: "IX_user_solution_language_id",
                table: "user_solution");

            migrationBuilder.DropColumn(
                name: "language_id",
                table: "user_solution");

            migrationBuilder.AlterColumn<byte>(
                name: "stars",
                table: "user_solution",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            
            migrationBuilder.Sql(@"
                ALTER TABLE user_solution 
                ALTER COLUMN code_runtime_submitted TYPE bigint 
                USING (EXTRACT(EPOCH FROM code_runtime_submitted) * 1000)::bigint;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "stars",
                table: "user_solution",
                type: "integer",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "code_runtime_submitted",
                table: "user_solution",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<Guid>(
                name: "language_id",
                table: "user_solution",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "language",
                columns: table => new
                {
                    language_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    version = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("language_pk", x => x.language_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_solution_language_id",
                table: "user_solution",
                column: "language_id");

            migrationBuilder.AddForeignKey(
                name: "user_solution_language_ref",
                table: "user_solution",
                column: "language_id",
                principalTable: "language",
                principalColumn: "language_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
