using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class addAuthApiKeyAndRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "api_key",
                columns: table => new
                {
                    api_key_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    prefix = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    key_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    key_salt = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("api_key_pk", x => x.api_key_id);
                    table.ForeignKey(
                        name: "api_key_application_user",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_token",
                columns: table => new
                {
                    refresh_token_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    token_salt = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    token_prefix = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("refresh_token_pk", x => x.refresh_token_id);
                    table.ForeignKey(
                        name: "refresh_token_application_user",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "refresh_token_replaced_by_token",
                        column: x => x.replaced_by_token_id,
                        principalTable: "refresh_token",
                        principalColumn: "refresh_token_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "refresh_token_session",
                        column: x => x.session_id,
                        principalTable: "session",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "api_key_prefix_idx",
                table: "api_key",
                column: "prefix");

            migrationBuilder.CreateIndex(
                name: "IX_api_key_user_id",
                table: "api_key",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_replaced_by_token_id",
                table: "refresh_token",
                column: "replaced_by_token_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_session_id",
                table: "refresh_token",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_user_id",
                table: "refresh_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "refresh_token_prefix_idx",
                table: "refresh_token",
                column: "token_prefix");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_key");

            migrationBuilder.DropTable(
                name: "refresh_token");
        }
    }
}
