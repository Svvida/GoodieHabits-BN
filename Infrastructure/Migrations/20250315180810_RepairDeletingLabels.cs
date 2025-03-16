using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RepairDeletingLabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestMetadata_QuestLabel_Quest_Labels_QuestLabelId",
                table: "QuestMetadata_QuestLabel");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestMetadata_QuestLabel_Quest_Labels_QuestLabelId",
                table: "QuestMetadata_QuestLabel",
                column: "QuestLabelId",
                principalTable: "Quest_Labels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestMetadata_QuestLabel_Quest_Labels_QuestLabelId",
                table: "QuestMetadata_QuestLabel");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestMetadata_QuestLabel_Quest_Labels_QuestLabelId",
                table: "QuestMetadata_QuestLabel",
                column: "QuestLabelId",
                principalTable: "Quest_Labels",
                principalColumn: "Id");
        }
    }
}
