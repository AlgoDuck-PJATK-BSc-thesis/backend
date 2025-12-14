using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    public partial class EnforceUserCohortRefSetNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_cohort_ref",
                table: "application_user");

            migrationBuilder.AddForeignKey(
                name: "user_cohort_ref",
                table: "application_user",
                column: "cohort_id",
                principalTable: "cohort",
                principalColumn: "cohort_id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_cohort_ref",
                table: "application_user");

            migrationBuilder.AddForeignKey(
                name: "user_cohort_ref",
                table: "application_user",
                column: "cohort_id",
                principalTable: "cohort",
                principalColumn: "cohort_id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}