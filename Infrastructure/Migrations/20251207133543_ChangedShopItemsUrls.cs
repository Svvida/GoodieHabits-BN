using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedShopItemsUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 100,
                column: "ImageUrl",
                value: "shop_items/avatar_frames/wooden_rookie");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 101,
                column: "ImageUrl",
                value: "shop_items/avatar_frames/silver_striver");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 102,
                column: "ImageUrl",
                value: "shop_items/avatar_frames/gold_grinder");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 103,
                column: "ImageUrl",
                value: "shop_items/avatar_frames/diamond_achiever");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 200,
                column: "ImageUrl",
                value: "shop_items/avatars/cat_programmer");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 201,
                column: "ImageUrl",
                value: "shop_items/avatars/giga_chad");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 202,
                column: "ImageUrl",
                value: "shop_items/avatars/wizard_deadlines");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 300,
                column: "ImageUrl",
                value: "shop_items/pets/tiny_dragon");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 301,
                column: "ImageUrl",
                value: "shop_items/pets/floating_duck");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 400,
                column: "ImageUrl",
                value: "shop_items/titles/procrastinator");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 401,
                column: "ImageUrl",
                value: "shop_items/titles/slayer");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 402,
                column: "ImageUrl",
                value: "shop_items/titles/lord_deadlines");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 500,
                column: "ImageUrl",
                value: "shop_items/name_effects/neon_blue");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 501,
                column: "ImageUrl",
                value: "shop_items/name_effects/inferno");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 600,
                column: "ImageUrl",
                value: "shop_items/consumables/snack");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 601,
                column: "ImageUrl",
                value: "shop_items/consumables/giga_meal");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 602,
                column: "ImageUrl",
                value: "shop_items/consumables/focus_brew");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 100,
                column: "ImageUrl",
                value: "frames/wooden_rookie");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 101,
                column: "ImageUrl",
                value: "frames/silver_striver");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 102,
                column: "ImageUrl",
                value: "frames/gold_grinder");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 103,
                column: "ImageUrl",
                value: "frames/diamond_achiever");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 200,
                column: "ImageUrl",
                value: "avatars/cat_programmer");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 201,
                column: "ImageUrl",
                value: "avatars/giga_chad");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 202,
                column: "ImageUrl",
                value: "avatars/wizard_deadlines");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 300,
                column: "ImageUrl",
                value: "pets/tiny_dragon");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 301,
                column: "ImageUrl",
                value: "pets/floating_duck");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 400,
                column: "ImageUrl",
                value: "titles/procrastinator");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 401,
                column: "ImageUrl",
                value: "titles/slayer");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 402,
                column: "ImageUrl",
                value: "titles/lord_deadlines");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 500,
                column: "ImageUrl",
                value: "nameeffects/neon_blue");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 501,
                column: "ImageUrl",
                value: "nameeffects/inferno");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 600,
                column: "ImageUrl",
                value: "consumables/snack");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 601,
                column: "ImageUrl",
                value: "consumables/giga_meal");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 602,
                column: "ImageUrl",
                value: "consumables/focus_brew");
        }
    }
}
