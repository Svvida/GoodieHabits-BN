using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HashPassword = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "Seasonal_Quests",
                columns: table => new
                {
                    SeasonalQuestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ActiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActiveTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Emoji = table.Column<string>(type: "NVARCHAR(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, computedColumnSql: "CASE WHEN GETUTCDATE() BETWEEN ActiveFrom AND ActiveTo THEN 1 ELSE 0 END"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasonal_Quests", x => x.SeasonalQuestId);
                });

            migrationBuilder.CreateTable(
                name: "One_Time_Quests",
                columns: table => new
                {
                    OneTimeQuestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Emoji = table.Column<string>(type: "NVARCHAR(10)", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_One_Time_Quests", x => x.OneTimeQuestId);
                    table.ForeignKey(
                        name: "FK_One_Time_Quests_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recurring_Quests",
                columns: table => new
                {
                    RecurringQuestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RepeatTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    RepeatIntervalJson = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    Emoji = table.Column<string>(type: "NVARCHAR(10)", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recurring_Quests", x => x.RecurringQuestId);
                    table.ForeignKey(
                        name: "FK_Recurring_Quests_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User_Seasonal_Quests",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    SeasonalQuestId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Seasonal_Quests", x => new { x.AccountId, x.SeasonalQuestId });
                    table.ForeignKey(
                        name: "FK_User_Seasonal_Quests_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_Seasonal_Quests_Seasonal_Quests_SeasonalQuestId",
                        column: x => x.SeasonalQuestId,
                        principalTable: "Seasonal_Quests",
                        principalColumn: "SeasonalQuestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email",
                table: "Accounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Username",
                table: "Accounts",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_One_Time_Quests_AccountId",
                table: "One_Time_Quests",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Recurring_Quests_AccountId",
                table: "Recurring_Quests",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Seasonal_Quests_SeasonalQuestId",
                table: "User_Seasonal_Quests",
                column: "SeasonalQuestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "One_Time_Quests");

            migrationBuilder.DropTable(
                name: "Recurring_Quests");

            migrationBuilder.DropTable(
                name: "User_Seasonal_Quests");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Seasonal_Quests");
        }
    }
}
