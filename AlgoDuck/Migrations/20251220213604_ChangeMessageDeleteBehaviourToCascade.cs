using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMessageDeleteBehaviourToCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assistance_message_assistant_chat_chat_name_problem_id_user~",
                table: "assistance_message");

            migrationBuilder.DropForeignKey(
                name: "FK_assistant_message_code_fragment_assistance_message_MessageId",
                table: "assistant_message_code_fragment");

            migrationBuilder.AddForeignKey(
                name: "FK_assistance_message_assistant_chat_chat_name_problem_id_user~",
                table: "assistance_message",
                columns: new[] { "chat_name", "problem_id", "user_id" },
                principalTable: "assistant_chat",
                principalColumns: new[] { "name", "problem_id", "user_id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_assistant_message_code_fragment_assistance_message_MessageId",
                table: "assistant_message_code_fragment",
                column: "MessageId",
                principalTable: "assistance_message",
                principalColumn: "message_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assistance_message_assistant_chat_chat_name_problem_id_user~",
                table: "assistance_message");

            migrationBuilder.DropForeignKey(
                name: "FK_assistant_message_code_fragment_assistance_message_MessageId",
                table: "assistant_message_code_fragment");

            migrationBuilder.AddForeignKey(
                name: "FK_assistance_message_assistant_chat_chat_name_problem_id_user~",
                table: "assistance_message",
                columns: new[] { "chat_name", "problem_id", "user_id" },
                principalTable: "assistant_chat",
                principalColumns: new[] { "name", "problem_id", "user_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_assistant_message_code_fragment_assistance_message_MessageId",
                table: "assistant_message_code_fragment",
                column: "MessageId",
                principalTable: "assistance_message",
                principalColumn: "message_id");
        }
    }
}
