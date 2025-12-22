using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class RenameCohortIsActiveToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_cohort_ref",
                table: "application_user");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "cohort",
                newName: "is_active");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "application_user",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "user_cohort_ref",
                table: "application_user",
                column: "cohort_id",
                principalTable: "cohort",
                principalColumn: "cohort_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_cohort_ref",
                table: "application_user");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "cohort",
                newName: "IsActive");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "application_user",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "user_cohort_ref",
                table: "application_user",
                column: "cohort_id",
                principalTable: "cohort",
                principalColumn: "cohort_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
