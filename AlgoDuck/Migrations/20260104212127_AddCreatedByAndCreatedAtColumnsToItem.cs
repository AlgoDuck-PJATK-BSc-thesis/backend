using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByAndCreatedAtColumnsToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Purchasable",
                table: "item",
                newName: "purchasable");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "item",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "RarityId",
                table: "item",
                newName: "rarity_id");

            migrationBuilder.RenameIndex(
                name: "IX_item_RarityId",
                table: "item",
                newName: "IX_item_rarity_id");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "item",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "item",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_item_CreatedById",
                table: "item",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_item_application_user_CreatedById",
                table: "item",
                column: "CreatedById",
                principalTable: "application_user",
                principalColumn: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_item_application_user_CreatedById",
                table: "item");

            migrationBuilder.DropIndex(
                name: "IX_item_CreatedById",
                table: "item");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "item");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "item");

            migrationBuilder.RenameColumn(
                name: "purchasable",
                table: "item",
                newName: "Purchasable");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "item",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "rarity_id",
                table: "item",
                newName: "RarityId");

            migrationBuilder.RenameIndex(
                name: "IX_item_rarity_id",
                table: "item",
                newName: "IX_item_RarityId");
        }
    }
}
