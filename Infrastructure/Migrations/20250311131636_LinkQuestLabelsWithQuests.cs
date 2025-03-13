using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkQuestLabelsWithQuests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Quest_Labels",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "TextColor",
                table: "Quest_Labels",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BackgroundColor",
                table: "Quest_Labels",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "QuestMetadata_QuestLabel",
                columns: table => new
                {
                    QuestMetadataId = table.Column<int>(type: "int", nullable: false),
                    QuestLabelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestMetadata_QuestLabel", x => new { x.QuestMetadataId, x.QuestLabelId });
                    table.ForeignKey(
                        name: "FK_QuestMetadata_QuestLabel_Quest_Labels_QuestLabelId",
                        column: x => x.QuestLabelId,
                        principalTable: "Quest_Labels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestMetadata_QuestLabel_Quests_QuestMetadataId",
                        column: x => x.QuestMetadataId,
                        principalTable: "Quests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestMetadata_QuestLabel_QuestLabelId",
                table: "QuestMetadata_QuestLabel",
                column: "QuestLabelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestMetadata_QuestLabel");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Quest_Labels",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "TextColor",
                table: "Quest_Labels",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(7)",
                oldMaxLength: 7);

            migrationBuilder.AlterColumn<string>(
                name: "BackgroundColor",
                table: "Quest_Labels",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(7)",
                oldMaxLength: 7);
        }
    }
}
