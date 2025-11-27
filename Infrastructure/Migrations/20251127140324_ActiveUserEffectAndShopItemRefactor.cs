using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActiveUserEffectAndShopItemRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EffectDataJson",
                table: "ShopItems",
                newName: "Payload");

            migrationBuilder.CreateTable(
                name: "ActiveUserEffects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    SourceItemId = table.Column<int>(type: "int", nullable: false),
                    EffectType = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: true),
                    EffectDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveUserEffects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveUserEffects_ShopItems_SourceItemId",
                        column: x => x.SourceItemId,
                        principalTable: "ShopItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActiveUserEffects_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 100,
                column: "Payload",
                value: "{\"type\":\"AvatarFrame\",\"FrameId\":\"wooden_rookie\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 101,
                column: "Payload",
                value: "{\"type\":\"AvatarFrame\",\"FrameId\":\"silver_striver\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 102,
                column: "Payload",
                value: "{\"type\":\"AvatarFrame\",\"FrameId\":\"gold_grinder\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 103,
                column: "Payload",
                value: "{\"type\":\"AvatarFrame\",\"FrameId\":\"diamond_achiever\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 200,
                column: "Payload",
                value: "{\"type\":\"Avatar\",\"AvatarId\":\"cat_programmer\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 201,
                column: "Payload",
                value: "{\"type\":\"Avatar\",\"AvatarId\":\"giga_chad\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 202,
                column: "Payload",
                value: "{\"type\":\"Avatar\",\"AvatarId\":\"wizard_deadlines\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 300,
                column: "Payload",
                value: "{\"type\":\"Pet\",\"PetId\":\"tiny_dragon\",\"Animation\":\"hover\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 301,
                column: "Payload",
                value: "{\"type\":\"Pet\",\"PetId\":\"floating_duck\",\"Animation\":\"float\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 400,
                column: "Payload",
                value: "{\"type\":\"Title\",\"TitleText\":\"Certified Procrastinator\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 401,
                column: "Payload",
                value: "{\"type\":\"Title\",\"TitleText\":\"Task Slayer\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 402,
                column: "Payload",
                value: "{\"type\":\"Title\",\"TitleText\":\"Lord of Deadlines\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 500,
                column: "Payload",
                value: "{\"type\":\"NameEffect\",\"EffectStyle\":\"glow\",\"ColorHex\":\"#00AFFF\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 501,
                column: "Payload",
                value: "{\"type\":\"NameEffect\",\"EffectStyle\":\"aura\",\"ColorHex\":\"#FF4500\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 600,
                column: "Payload",
                value: "{\"type\":\"Consumable\",\"EffectType\":2,\"DurationMinutes\":null,\"UsageCount\":1,\"Multiplier\":null,\"FlatValue\":40}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 601,
                columns: new[] { "Description", "Payload" },
                values: new object[] { "+120 XP on next completed task.", "{\"type\":\"Consumable\",\"EffectType\":2,\"DurationMinutes\":null,\"UsageCount\":1,\"Multiplier\":null,\"FlatValue\":120}" });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 602,
                column: "Payload",
                value: "{\"type\":\"Consumable\",\"EffectType\":0,\"DurationMinutes\":null,\"UsageCount\":2,\"Multiplier\":2,\"FlatValue\":null}");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveUserEffects_SourceItemId",
                table: "ActiveUserEffects",
                column: "SourceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveUserEffects_UserProfileId",
                table: "ActiveUserEffects",
                column: "UserProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveUserEffects");

            migrationBuilder.RenameColumn(
                name: "Payload",
                table: "ShopItems",
                newName: "EffectDataJson");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 100,
                column: "EffectDataJson",
                value: "{\"frameId\": \"wooden_rookie\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 101,
                column: "EffectDataJson",
                value: "{\"frameId\": \"silver_striver\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 102,
                column: "EffectDataJson",
                value: "{\"frameId\": \"gold_grinder\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 103,
                column: "EffectDataJson",
                value: "{\"frameId\": \"diamond_achiever\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 200,
                column: "EffectDataJson",
                value: "{\"avatarId\": \"cat_programmer\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 201,
                column: "EffectDataJson",
                value: "{\"avatarId\": \"giga_chad\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 202,
                column: "EffectDataJson",
                value: "{\"avatarId\": \"wizard_deadlines\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 300,
                column: "EffectDataJson",
                value: "{\"petId\": \"tiny_dragon\", \"animation\": \"hover\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 301,
                column: "EffectDataJson",
                value: "{\"petId\": \"floating_duck\", \"animation\": \"float\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 400,
                column: "EffectDataJson",
                value: "{\"title\": \"Certified Procrastinator\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 401,
                column: "EffectDataJson",
                value: "{\"title\": \"Task Slayer\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 402,
                column: "EffectDataJson",
                value: "{\"title\": \"Lord of Deadlines\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 500,
                column: "EffectDataJson",
                value: "{\"effect\": \"glow\", \"color\": \"#00AFFF\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 501,
                column: "EffectDataJson",
                value: "{\"effect\": \"aura\", \"style\": \"fire\"}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 600,
                column: "EffectDataJson",
                value: "{\"type\": \"xp\", \"amount\": 40}");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 601,
                columns: new[] { "Description", "EffectDataJson" },
                values: new object[] { "+350 XP on next completed task.", "{\"type\": \"xp\", \"amount\": 350}" });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 602,
                column: "EffectDataJson",
                value: "{\"type\": \"multiplier\", \"x\": 2, \"tasks\": 2}");
        }
    }
}
