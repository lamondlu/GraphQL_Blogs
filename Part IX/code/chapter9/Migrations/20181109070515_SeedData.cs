using Microsoft.EntityFrameworkCore.Migrations;

namespace chapter1.Migrations
{
    public partial class SeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Barcode", "SellingPrice", "Title" },
                values: new object[] { "123", 50m, "Headphone" });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Barcode", "SellingPrice", "Title" },
                values: new object[] { "456", 40m, "Keyboard" });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Barcode", "SellingPrice", "Title" },
                values: new object[] { "789", 100m, "Monitor" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Barcode",
                keyValue: "123");

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Barcode",
                keyValue: "456");

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Barcode",
                keyValue: "789");
        }
    }
}
