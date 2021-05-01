using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Sceny.Finance.WebApp.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_type",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 128, nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false)
                },
                constraints: table => table.PrimaryKey("pk_account_type", x => x.id));

            migrationBuilder.CreateTable(
                name: "currency",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(maxLength: 128, nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false)
                },
                constraints: table => table.PrimaryKey("pk_currency", x => x.id));

            migrationBuilder.CreateTable(
                name: "transaction_kind",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 32, nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false)
                },
                constraints: table => table.PrimaryKey("pk_transaction_kind", x => x.id));

            migrationBuilder.CreateTable(
                name: "transaction_status",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 32, nullable: false)
                },
                constraints: table => table.PrimaryKey("pk_transaction_status", x => x.id));
            migrationBuilder.CreateTable(
                name: "account",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(maxLength: 128, nullable: false),
                    currency_id = table.Column<int>(nullable: false),
                    account_type_id = table.Column<int>(nullable: false),
                    account_type_id1 = table.Column<int>(nullable: false),
                    institution = table.Column<string>(maxLength: 128, nullable: true),
                    number = table.Column<string>(maxLength: 128, nullable: true),
                    xmin = table.Column<uint>(type: "xid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account", x => x.id);
                    table.ForeignKey(
                        name: "fk_account_account_type_account_type_id1",
                        column: x => x.account_type_id1,
                        principalTable: "account_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_account_currency_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(maxLength: 128, nullable: false),
                    kind_id = table.Column<int>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category", x => x.id);
                    table.ForeignKey(
                        name: "fk_category_transaction_kind_kind_id",
                        column: x => x.kind_id,
                        principalTable: "transaction_kind",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "budget_item",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    month = table.Column<DateTime>(nullable: false),
                    category_id = table.Column<int>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_budget_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_budget_item_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaction_item",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateTime>(nullable: false),
                    amount = table.Column<double>(nullable: false),
                    memo = table.Column<string>(maxLength: 2000, nullable: false),
                    information = table.Column<string>(maxLength: 2000, nullable: false),
                    from_account_id = table.Column<int>(nullable: false),
                    to_account_id = table.Column<int>(nullable: true),
                    category_id = table.Column<int>(nullable: false),
                    kind_id = table.Column<int>(nullable: false),
                    status_id = table.Column<int>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaction_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_transaction_item_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transaction_item_account_from_account_id",
                        column: x => x.from_account_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transaction_item_transaction_kind_kind_id",
                        column: x => x.kind_id,
                        principalTable: "transaction_kind",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transaction_item_transaction_status_status_id",
                        column: x => x.status_id,
                        principalTable: "transaction_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transaction_item_account_to_account_id",
                        column: x => x.to_account_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "account_type",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Other" },
                    { 2, "Bank" },
                    { 3, "Cash" },
                    { 4, "Credit Card" }
                });

            migrationBuilder.InsertData(
                table: "transaction_kind",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Income" },
                    { 2, "Expense" },
                    { 3, "Transfer" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_account_type_id1",
                table: "account",
                column: "account_type_id1");

            migrationBuilder.CreateIndex(
                name: "ix_account_currency_id",
                table: "account",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "ix_budget_item_category_id",
                table: "budget_item",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_category_kind_id",
                table: "category",
                column: "kind_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_item_category_id",
                table: "transaction_item",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_item_from_account_id",
                table: "transaction_item",
                column: "from_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_item_kind_id",
                table: "transaction_item",
                column: "kind_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_item_status_id",
                table: "transaction_item",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_item_to_account_id",
                table: "transaction_item",
                column: "to_account_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "budget_item");

            migrationBuilder.DropTable(
                name: "transaction_item");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "account");

            migrationBuilder.DropTable(
                name: "transaction_status");

            migrationBuilder.DropTable(
                name: "transaction_kind");

            migrationBuilder.DropTable(
                name: "account_type");

            migrationBuilder.DropTable(
                name: "currency");
        }
    }
}
