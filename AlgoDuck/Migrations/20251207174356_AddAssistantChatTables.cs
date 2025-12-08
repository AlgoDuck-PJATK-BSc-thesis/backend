using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class AddAssistantChatTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assistant_chat",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    problem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("assistant_chat_id", x => new { x.name, x.problem_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_assistant_chat_application_user_user_id",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_assistant_chat_problem_problem_id",
                        column: x => x.problem_id,
                        principalTable: "problem",
                        principalColumn: "problem_id");
                });

            migrationBuilder.CreateTable(
                name: "assistance_message",
                columns: table => new
                {
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assistance_message", x => x.message_id);
                    table.ForeignKey(
                        name: "FK_assistance_message_assistant_chat_ChatName_ProblemId_UserId",
                        columns: x => new { x.ChatName, x.ProblemId, x.UserId },
                        principalTable: "assistant_chat",
                        principalColumns: new[] { "name", "problem_id", "user_id" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_assistance_message_ChatName_ProblemId_UserId",
                table: "assistance_message",
                columns: new[] { "ChatName", "ProblemId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_assistant_chat_problem_id",
                table: "assistant_chat",
                column: "problem_id");

            migrationBuilder.CreateIndex(
                name: "IX_assistant_chat_user_id",
                table: "assistant_chat",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assistance_message");

            migrationBuilder.DropTable(
                name: "assistant_chat");
        }
    }
}
