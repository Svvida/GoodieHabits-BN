using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDeletingQuestLabelsRelationshipWithQuests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestMetadata_QuestLabel_Quest_Labels_QuestLabelId",
                table: "QuestMetadata_QuestLabel");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestMetadata_QuestLabel_Quests_QuestMetadataId",
                table: "QuestMetadata_QuestLabel");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestMetadata_QuestLabel_Quest_Labels_QuestLabelId",
                table: "QuestMetadata_QuestLabel",
                column: "QuestLabelId",
                principalTable: "Quest_Labels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestMetadata_QuestLabel_Quests_QuestMetadataId",
                table: "QuestMetadata_QuestLabel",
                column: "QuestMetadataId",
                principalTable: "Quests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestMetadata_QuestLabel_Quest_Labels_QuestLabelId",
                table: "QuestMetadata_QuestLabel");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestMetadata_QuestLabel_Quests_QuestMetadataId",
                table: "QuestMetadata_QuestLabel");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestMetadata_QuestLabel_Quest_Labels_QuestLabelId",
                table: "QuestMetadata_QuestLabel",
                column: "QuestLabelId",
                principalTable: "Quest_Labels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestMetadata_QuestLabel_Quests_QuestMetadataId",
                table: "QuestMetadata_QuestLabel",
                column: "QuestMetadataId",
                principalTable: "Quests",
                principalColumn: "Id");
        }
    }
}
