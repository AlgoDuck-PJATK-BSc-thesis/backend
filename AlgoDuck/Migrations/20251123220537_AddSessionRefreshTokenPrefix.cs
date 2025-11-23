using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionRefreshTokenPrefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "refresh_token_prefix",
                table: "session",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "session_refresh_prefix_idx",
                table: "session",
                column: "refresh_token_prefix");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "session_refresh_prefix_idx",
                table: "session");

            migrationBuilder.DropColumn(
                name: "refresh_token_prefix",
                table: "session");
        }
    }
}
