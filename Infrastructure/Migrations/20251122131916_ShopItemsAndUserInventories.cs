using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ShopItemsAndUserInventories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Coins",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateTable(
                name: "ShopItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    CurrencyType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LevelRequirement = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsPurchasable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsUnique = table.Column<bool>(type: "bit", nullable: false),
                    EffectDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    ShopItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    AcquiredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInventories_ShopItems_ShopItemId",
                        column: x => x.ShopItemId,
                        principalTable: "ShopItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserInventories_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ShopItems",
                columns: new[] { "Id", "Category", "Description", "EffectDataJson", "ImageUrl", "IsUnique", "ItemType", "LevelRequirement", "Name", "Price" },
                values: new object[,]
                {
                    { 100, 0, "Unlocked at Level 1. Made from 100% imaginary wood.", "{\"frameId\": \"wooden_rookie\"}", "frames/wooden_rookie", true, 0, 1, "Wooden Rookie Frame", 0 },
                    { 101, 0, "Unlocked at Level 4. Clean, shiny, probably not real silver.", "{\"frameId\": \"silver_striver\"}", "frames/silver_striver", true, 0, 4, "Silver Striver Frame", 0 },
                    { 102, 0, "Unlocked at Level 8. You earned this—literally.", "{\"frameId\": \"gold_grinder\"}", "frames/gold_grinder", true, 0, 8, "Gold Grinder Frame", 0 },
                    { 103, 0, "Unlocked at Level 15. Shiny enough to motivate anyone.", "{\"frameId\": \"diamond_achiever\"}", "frames/diamond_achiever", true, 0, 15, "Diamond Achiever Frame", 0 }
                });

            migrationBuilder.InsertData(
                table: "ShopItems",
                columns: new[] { "Id", "Category", "Description", "EffectDataJson", "ImageUrl", "IsPurchasable", "IsUnique", "ItemType", "LevelRequirement", "Name", "Price" },
                values: new object[,]
                {
                    { 200, 1, "A cat furiously typing on a keyboard. Appears to know more than you.", "{\"avatarId\": \"cat_programmer\"}", "avatars/cat_programmer", true, true, 2, 1, "Cat Programmer", 300 },
                    { 201, 1, "The peak of digital evolution. Jawline DLC included.", "{\"avatarId\": \"giga_chad\"}", "avatars/giga_chad", true, true, 2, 5, "Giga-Chad Avatar", 400 },
                    { 202, 1, "A stressed wizard constantly casting 'EXTEND DEADLINE' spell.", "{\"avatarId\": \"wizard_deadlines\"}", "avatars/wizard_deadlines", true, true, 2, 3, "Wizard of Deadlines", 350 },
                    { 300, 5, "A baby dragon that follows you around. Harmless. Probably.", "{\"petId\": \"tiny_dragon\", \"animation\": \"hover\"}", "pets/tiny_dragon", true, true, 5, 10, "Tiny Dragon", 1200 },
                    { 301, 5, "A yellow duck that defies gravity. Scientists hate it.", "{\"petId\": \"floating_duck\", \"animation\": \"float\"}", "pets/floating_duck", true, true, 5, 6, "Floating Duck", 800 },
                    { 400, 6, "A title that proudly states what everyone already knew.", "{\"title\": \"Certified Procrastinator\"}", "titles/procrastinator", true, true, 6, 1, "Certified Procrastinator", 250 },
                    { 401, 6, "For those who finish tasks with extreme prejudice.", "{\"title\": \"Task Slayer\"}", "titles/slayer", true, true, 6, 8, "Task Slayer", 600 },
                    { 402, 6, "One title to postpone them all.", "{\"title\": \"Lord of Deadlines\"}", "titles/lord_deadlines", true, true, 6, 12, "Lord of Deadlines", 900 },
                    { 500, 3, "Makes your username glow with neon blue energy.", "{\"effect\": \"glow\", \"color\": \"#00AFFF\"}", "nameeffects/neon_blue", true, true, 8, 4, "Blue Neon Name Glow", 500 },
                    { 501, 3, "Your username ignites with flame animation.", "{\"effect\": \"aura\", \"style\": \"fire\"}", "nameeffects/inferno", true, true, 8, 10, "Inferno Name Aura", 950 },
                    { 600, 4, "+40 XP on next completed task. Tastes digital.", "{\"type\": \"xp\", \"amount\": 40}", "consumables/snack", true, false, 1, 1, "Mini XP Snack", 120 },
                    { 601, 4, "+350 XP on next completed task.", "{\"type\": \"xp\", \"amount\": 350}", "consumables/giga_meal", true, false, 1, 6, "Giga XP Meal", 450 },
                    { 602, 4, "Doubles XP for the next 2 tasks.", "{\"type\": \"multiplier\", \"x\": 2, \"tasks\": 2}", "consumables/focus_brew", true, false, 1, 10, "Ultra Focus Brew", 800 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Id_UserProfileId",
                table: "Notifications",
                columns: new[] { "Id", "UserProfileId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopItems_Category",
                table: "ShopItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_UserInventories_ShopItemId",
                table: "UserInventories",
                column: "ShopItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInventories_UserProfileId_ShopItemId",
                table: "UserInventories",
                columns: new[] { "UserProfileId", "ShopItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserInventories");

            migrationBuilder.DropTable(
                name: "ShopItems");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Id_UserProfileId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Coins",
                table: "UserProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "Notifications",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");
        }
    }
}
