using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class OnDeleteCascadeUserConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_config_application_user",
                table: "user_config");

            migrationBuilder.AddForeignKey(
                name: "user_config_application_user",
                table: "user_config",
                column: "user_id",
                principalTable: "application_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_config_application_user",
                table: "user_config");

            migrationBuilder.AddForeignKey(
                name: "user_config_application_user",
                table: "user_config",
                column: "user_id",
                principalTable: "application_user",
                principalColumn: "user_id");
        }
    }
}
