using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailAndPushNotificationsUserConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarKey",
                table: "user_config");

            migrationBuilder.RenameColumn(
                name: "Language",
                table: "user_config",
                newName: "language");

            migrationBuilder.AlterColumn<string>(
                name: "language",
                table: "user_config",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "email_notifications_enabled",
                table: "user_config",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "push_notifications_enabled",
                table: "user_config",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email_notifications_enabled",
                table: "user_config");

            migrationBuilder.DropColumn(
                name: "push_notifications_enabled",
                table: "user_config");

            migrationBuilder.RenameColumn(
                name: "language",
                table: "user_config",
                newName: "Language");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "user_config",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16);

            migrationBuilder.AddColumn<string>(
                name: "AvatarKey",
                table: "user_config",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
