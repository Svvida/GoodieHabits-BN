using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFinanceSystemCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#1987EE", "home-outline", "Mieszkanie" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#F59E0B", "car-outline", "Transport" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#10B981", "heart-outline", "Życie i Zdrowie" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#8B5CF6", "school-outline", "Rozwój i Edukacja" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#EC4899", "game-controller-outline", "Rozrywka i Inne" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Color", "Icon", "IsSavings", "Name" },
                values: new object[] { "#14B8A6", "trending-up-outline", true, "Finanse i Oszczędności" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#10B981", "briefcase-outline", "Wynagrodzenie" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#10B981", "gift-outline", "Premia / Bonus" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 52,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#10B981", "business-outline", "Działalność gosp." });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#10B981", "laptop-outline", "Freelance" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#10B981", "wallet-outline", "Dochód pasywny" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 101,
                column: "Name",
                value: "Czynsz / Rata kredytu");

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 102,
                column: "Name",
                value: "Czynsz administracyjny");

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 103,
                column: "Name",
                value: "Prąd");

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 104,
                column: "Name",
                value: "Woda i ścieki");

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "Name", "ParentCategoryId" },
                values: new object[] { "Gaz", 1 });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "Name", "ParentCategoryId" },
                values: new object[] { "Ogrzewanie", 1 });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 107,
                columns: new[] { "Name", "ParentCategoryId" },
                values: new object[] { "Wywóz nieczystości", 1 });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 108,
                columns: new[] { "Name", "ParentCategoryId" },
                values: new object[] { "Internet / Wi-Fi", 1 });

            migrationBuilder.InsertData(
                table: "FinanceCategories",
                columns: new[] { "Id", "Color", "CreatedAt", "Icon", "IsSystem", "Name", "ParentCategoryId", "Type", "UpdatedAt", "UserProfileId" },
                values: new object[,]
                {
                    { 55, "#10B981", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "shield-checkmark-outline", true, "Świadczenia", null, 0, null, null },
                    { 56, "#10B981", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "receipt-outline", true, "Zwrot podatku", null, 0, null, null },
                    { 57, "#10B981", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ellipsis-horizontal-outline", true, "Inne", null, 0, null, null },
                    { 109, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Telewizja", 1, 1, null, null },
                    { 110, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Ubezpieczenie nieruchomości", 1, 1, null, null },
                    { 111, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Serwis / Naprawy domowe", 1, 1, null, null },
                    { 112, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Środki czystości", 1, 1, null, null },
                    { 113, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Paliwo", 2, 1, null, null },
                    { 114, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Rata kredytu / leasingu", 2, 1, null, null },
                    { 115, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Ubezpieczenie OC/AC", 2, 1, null, null },
                    { 116, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Przegląd / Serwis", 2, 1, null, null },
                    { 117, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Opony", 2, 1, null, null },
                    { 118, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Komunikacja miejska / PKP", 2, 1, null, null },
                    { 119, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Taxi / Uber / Bolt", 2, 1, null, null },
                    { 120, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Parkingi / Autostrady", 2, 1, null, null },
                    { 121, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Akcesoria samochodowe", 2, 1, null, null },
                    { 122, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Zakupy spożywcze", 3, 1, null, null },
                    { 123, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Jedzenie na mieście", 3, 1, null, null },
                    { 124, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Wizyty lekarskie", 3, 1, null, null },
                    { 125, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Leki i suplementy", 3, 1, null, null },
                    { 126, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Dentysta", 3, 1, null, null },
                    { 127, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Siłownia / Karnet sportowy", 3, 1, null, null },
                    { 128, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Kosmetyczka / Fryzjer", 3, 1, null, null },
                    { 129, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Odzież i obuwie", 3, 1, null, null },
                    { 130, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Czesne", 4, 1, null, null },
                    { 131, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Kursy online / Szkolenia", 4, 1, null, null },
                    { 132, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Książki / E-booki", 4, 1, null, null },
                    { 133, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Subskrypcje edukacyjne", 4, 1, null, null },
                    { 134, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Sprzęt edukacyjny", 4, 1, null, null },
                    { 135, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Streaming (Netflix, Spotify...)", 5, 1, null, null },
                    { 136, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Kino / Teatr / Koncerty", 5, 1, null, null },
                    { 137, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Hobby i akcesoria", 5, 1, null, null },
                    { 138, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Zwierzęta", 5, 1, null, null },
                    { 139, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Dzieci", 5, 1, null, null },
                    { 140, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Prezenty", 5, 1, null, null },
                    { 141, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Wyjścia ze znajomymi", 5, 1, null, null },
                    { 142, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Nieprzewidziane wydatki", 5, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "FinanceCategories",
                columns: new[] { "Id", "Color", "CreatedAt", "Icon", "IsSavings", "IsSystem", "Name", "ParentCategoryId", "Type", "UpdatedAt", "UserProfileId" },
                values: new object[,]
                {
                    { 143, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, true, "Poduszka finansowa", 6, 1, null, null },
                    { 144, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, true, "IKE / IKZE", 6, 1, null, null },
                    { 145, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, true, "Inwestycje (ETF / Giełda)", 6, 1, null, null },
                    { 146, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, true, "Oszczędności celowe", 6, 1, null, null },
                    { 147, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, true, "Spłata długów", 6, 1, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 118);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 119);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 120);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 124);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 125);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 126);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 127);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 128);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 129);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 130);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 131);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 132);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 133);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 134);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 135);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 136);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 137);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 138);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 139);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 140);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 141);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 142);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 143);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 144);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 145);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 146);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 147);

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#4E79A7", "home", "Home" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#F28E2B", "food", "Food" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#E15759", "car", "Transport" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#76B7B2", "health", "Health" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#59A14F", "entertainment", "Entertainment" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Color", "Icon", "IsSavings", "Name" },
                values: new object[] { "#EDC948", "shopping", false, "Shopping" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#59A14F", "salary", "Salary" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#8CD17D", "bonus", "Bonus" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 52,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#B6992D", "gift", "Gifts" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#499894", "investment", "Investments" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "Color", "Icon", "Name" },
                values: new object[] { "#86BCB6", "dots", "Other" });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 101,
                column: "Name",
                value: "Rent");

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 102,
                column: "Name",
                value: "Internet");

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 103,
                column: "Name",
                value: "Electricity");

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 104,
                column: "Name",
                value: "Water");

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "Name", "ParentCategoryId" },
                values: new object[] { "Groceries", 2 });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "Name", "ParentCategoryId" },
                values: new object[] { "Restaurants", 2 });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 107,
                columns: new[] { "Name", "ParentCategoryId" },
                values: new object[] { "Fuel", 3 });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 108,
                columns: new[] { "Name", "ParentCategoryId" },
                values: new object[] { "Public transport", 3 });

            migrationBuilder.InsertData(
                table: "FinanceCategories",
                columns: new[] { "Id", "Color", "CreatedAt", "Icon", "IsSystem", "Name", "ParentCategoryId", "Type", "UpdatedAt", "UserProfileId" },
                values: new object[] { 7, "#B07AA1", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "dots", true, "Other", null, 1, null, null });
        }
    }
}
