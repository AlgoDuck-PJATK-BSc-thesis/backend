using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMessageContentsFromStringToFragmentListToMakeCodeAndTextDifferantiationEasier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content",
                table: "assistance_message");

            migrationBuilder.CreateTable(
                name: "assistant_message_code_fragment",
                columns: table => new
                {
                    fragment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    fragment_content = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    fragment_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assistant_message_code_fragment", x => x.fragment_id);
                    table.ForeignKey(
                        name: "FK_assistant_message_code_fragment_assistance_message_MessageId",
                        column: x => x.MessageId,
                        principalTable: "assistance_message",
                        principalColumn: "message_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_assistant_message_code_fragment_MessageId",
                table: "assistant_message_code_fragment",
                column: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assistant_message_code_fragment");

            migrationBuilder.AddColumn<string>(
                name: "content",
                table: "assistance_message",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
