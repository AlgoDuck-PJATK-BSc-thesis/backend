using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class MinorMigrationToSootheEf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
