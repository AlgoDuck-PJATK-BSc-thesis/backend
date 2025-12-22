using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAssistantChatPkFromCompositeToSurrogate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assistance_message_assistant_chat_chat_name_problem_id_user~",
                table: "assistance_message");

            migrationBuilder.DropPrimaryKey(
                name: "assistant_chat_id",
                table: "assistant_chat");

            migrationBuilder.DropIndex(
                name: "IX_assistance_message_chat_name_problem_id_user_id",
                table: "assistance_message");

            migrationBuilder.DropColumn(
                name: "chat_name",
                table: "assistance_message");

            migrationBuilder.DropColumn(
                name: "problem_id",
                table: "assistance_message");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "assistance_message",
                newName: "ChatId");

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "assistant_chat",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "assistant_chat_id",
                table: "assistant_chat",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_assistance_message_ChatId",
                table: "assistance_message",
                column: "ChatId");

            migrationBuilder.AddForeignKey(
                name: "FK_assistance_message_assistant_chat_ChatId",
                table: "assistance_message",
                column: "ChatId",
                principalTable: "assistant_chat",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assistance_message_assistant_chat_ChatId",
                table: "assistance_message");

            migrationBuilder.DropPrimaryKey(
                name: "assistant_chat_id",
                table: "assistant_chat");

            migrationBuilder.DropIndex(
                name: "IX_assistance_message_ChatId",
                table: "assistance_message");

            migrationBuilder.DropColumn(
                name: "id",
                table: "assistant_chat");

            migrationBuilder.RenameColumn(
                name: "ChatId",
                table: "assistance_message",
                newName: "user_id");

            migrationBuilder.AddColumn<string>(
                name: "chat_name",
                table: "assistance_message",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "problem_id",
                table: "assistance_message",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "assistant_chat_id",
                table: "assistant_chat",
                columns: new[] { "name", "problem_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "IX_assistance_message_chat_name_problem_id_user_id",
                table: "assistance_message",
                columns: new[] { "chat_name", "problem_id", "user_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_assistance_message_assistant_chat_chat_name_problem_id_user~",
                table: "assistance_message",
                columns: new[] { "chat_name", "problem_id", "user_id" },
                principalTable: "assistant_chat",
                principalColumns: new[] { "name", "problem_id", "user_id" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
