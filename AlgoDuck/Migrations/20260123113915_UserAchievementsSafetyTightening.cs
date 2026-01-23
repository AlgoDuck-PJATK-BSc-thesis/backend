using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class UserAchievementsSafetyTightening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_user_achievement_user_id",
                table: "user_achievement",
                newName: "user_achievement_user_id_ix");

            migrationBuilder.AlterColumn<bool>(
                name: "is_completed",
                table: "user_achievement",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "current_value",
                table: "user_achievement",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "user_achievement_is_completed_ix",
                table: "user_achievement",
                column: "is_completed");

            migrationBuilder.CreateIndex(
                name: "user_achievement_user_code_uq",
                table: "user_achievement",
                columns: new[] { "user_id", "code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "user_achievement_is_completed_ix",
                table: "user_achievement");

            migrationBuilder.DropIndex(
                name: "user_achievement_user_code_uq",
                table: "user_achievement");

            migrationBuilder.RenameIndex(
                name: "user_achievement_user_id_ix",
                table: "user_achievement",
                newName: "IX_user_achievement_user_id");

            migrationBuilder.AlterColumn<bool>(
                name: "is_completed",
                table: "user_achievement",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "current_value",
                table: "user_achievement",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);
        }
    }
}
