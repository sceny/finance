using Microsoft.EntityFrameworkCore.Migrations;

namespace Sceny.Finance.WebApp.Migrations
{
    public partial class transaction_status : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "transaction_status",
                type: "xid",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.InsertData(
                table: "transaction_status",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "None" },
                    { 2, "Pending" },
                    { 3, "Cleared" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "transaction_status",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "transaction_status",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "transaction_status",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "transaction_status");
        }
    }
}
