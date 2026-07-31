using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionCorrections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CorrectedAmount",
                table: "FinanceTransactions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CorrectsTransactionId",
                table: "FinanceTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinanceTransactions_CorrectsTransactionId",
                table: "FinanceTransactions",
                column: "CorrectsTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinanceTransactions_FinanceTransactions_CorrectsTransactionId",
                table: "FinanceTransactions",
                column: "CorrectsTransactionId",
                principalTable: "FinanceTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinanceTransactions_FinanceTransactions_CorrectsTransactionId",
                table: "FinanceTransactions");

            migrationBuilder.DropIndex(
                name: "IX_FinanceTransactions_CorrectsTransactionId",
                table: "FinanceTransactions");

            migrationBuilder.DropColumn(
                name: "CorrectedAmount",
                table: "FinanceTransactions");

            migrationBuilder.DropColumn(
                name: "CorrectsTransactionId",
                table: "FinanceTransactions");
        }
    }
}
