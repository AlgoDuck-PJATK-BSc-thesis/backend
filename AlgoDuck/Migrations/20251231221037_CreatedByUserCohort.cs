using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class CreatedByUserCohort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cohort_application_user_created_by_user_id",
                table: "cohort");

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by_user_id",
                table: "cohort",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_cohort_application_user_created_by_user_id",
                table: "cohort",
                column: "created_by_user_id",
                principalTable: "application_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cohort_application_user_created_by_user_id",
                table: "cohort");

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by_user_id",
                table: "cohort",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_cohort_application_user_created_by_user_id",
                table: "cohort",
                column: "created_by_user_id",
                principalTable: "application_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
