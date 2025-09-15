using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class AddCohortChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Cohorts_CohortId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_user_UserId",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Messages",
                table: "Messages");

            migrationBuilder.RenameTable(
                name: "Messages",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "message",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "message",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "message",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CohortId",
                table: "message",
                newName: "cohort_id");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "message",
                newName: "message_id");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_UserId",
                table: "message",
                newName: "IX_message_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_CohortId",
                table: "message",
                newName: "IX_message_cohort_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_message",
                table: "message",
                column: "message_id");

            migrationBuilder.AddForeignKey(
                name: "FK_message_Cohorts_cohort_id",
                table: "message",
                column: "cohort_id",
                principalTable: "Cohorts",
                principalColumn: "CohortId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_message_user_user_id",
                table: "message",
                column: "user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_message_Cohorts_cohort_id",
                table: "message");

            migrationBuilder.DropForeignKey(
                name: "FK_message_user_user_id",
                table: "message");

            migrationBuilder.DropPrimaryKey(
                name: "PK_message",
                table: "message");

            migrationBuilder.RenameTable(
                name: "message",
                newName: "Messages");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "Messages",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Messages",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Messages",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "cohort_id",
                table: "Messages",
                newName: "CohortId");

            migrationBuilder.RenameColumn(
                name: "message_id",
                table: "Messages",
                newName: "MessageId");

            migrationBuilder.RenameIndex(
                name: "IX_message_user_id",
                table: "Messages",
                newName: "IX_Messages_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_message_cohort_id",
                table: "Messages",
                newName: "IX_Messages_CohortId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Messages",
                table: "Messages",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Cohorts_CohortId",
                table: "Messages",
                column: "CohortId",
                principalTable: "Cohorts",
                principalColumn: "CohortId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_user_UserId",
                table: "Messages",
                column: "UserId",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
