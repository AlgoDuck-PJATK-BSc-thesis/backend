using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class AddIsUserMessageColumnToMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assistance_message_assistant_chat_ChatName_ProblemId_UserId",
                table: "assistance_message");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "assistance_message",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "assistance_message",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ProblemId",
                table: "assistance_message",
                newName: "problem_id");

            migrationBuilder.RenameColumn(
                name: "ChatName",
                table: "assistance_message",
                newName: "chat_name");

            migrationBuilder.RenameIndex(
                name: "IX_assistance_message_ChatName_ProblemId_UserId",
                table: "assistance_message",
                newName: "IX_assistance_message_chat_name_problem_id_user_id");

            migrationBuilder.AddColumn<bool>(
                name: "is_user_message",
                table: "assistance_message",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_assistance_message_assistant_chat_chat_name_problem_id_user~",
                table: "assistance_message",
                columns: new[] { "chat_name", "problem_id", "user_id" },
                principalTable: "assistant_chat",
                principalColumns: new[] { "name", "problem_id", "user_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assistance_message_assistant_chat_chat_name_problem_id_user~",
                table: "assistance_message");

            migrationBuilder.DropColumn(
                name: "is_user_message",
                table: "assistance_message");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "assistance_message",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "assistance_message",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "problem_id",
                table: "assistance_message",
                newName: "ProblemId");

            migrationBuilder.RenameColumn(
                name: "chat_name",
                table: "assistance_message",
                newName: "ChatName");

            migrationBuilder.RenameIndex(
                name: "IX_assistance_message_chat_name_problem_id_user_id",
                table: "assistance_message",
                newName: "IX_assistance_message_ChatName_ProblemId_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_assistance_message_assistant_chat_ChatName_ProblemId_UserId",
                table: "assistance_message",
                columns: new[] { "ChatName", "ProblemId", "UserId" },
                principalTable: "assistant_chat",
                principalColumns: new[] { "name", "problem_id", "user_id" });
        }
    }
}
