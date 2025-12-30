using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class MakeItemAndItemOwnershipTablesTPHPolymorphic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "selected",
                table: "purchase");

            migrationBuilder.AddColumn<bool>(
                name: "duck_selected_as_avatar",
                table: "purchase",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "duck_selected_for_pond",
                table: "purchase",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ownership_type",
                table: "purchase",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte>(
                name: "plant_grid_x",
                table: "purchase",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "plant_grid_y",
                table: "purchase",
                type: "smallint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "item",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<byte>(
                name: "plant_height",
                table: "item",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "plant_width",
                table: "item",
                type: "smallint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "duck_selected_as_avatar",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "duck_selected_for_pond",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "ownership_type",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "plant_grid_x",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "plant_grid_y",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "plant_height",
                table: "item");

            migrationBuilder.DropColumn(
                name: "plant_width",
                table: "item");

            migrationBuilder.AddColumn<bool>(
                name: "selected",
                table: "purchase",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "item",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5);
        }
    }
}
