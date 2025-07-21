using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaLink_API.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_PharmacyStocks_DrugId_PharmacyId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_PharmacyStocks_DrugId_PharmacyId",
                table: "OrderDetail");

            migrationBuilder.DropTable(
                name: "PharmacyStocks");

            migrationBuilder.CreateTable(
                name: "PharmacyStock",
                columns: table => new
                {
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    PharmacyId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuantityAvailable = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyStock", x => new { x.PharmacyId, x.DrugId });
                    table.ForeignKey(
                        name: "FK_PharmacyStock_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "DrugID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PharmacyStock_Pharmacies_PharmacyId",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacies",
                        principalColumn: "PharmacyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyStock_DrugId",
                table: "PharmacyStock",
                column: "DrugId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_PharmacyStock_DrugId_PharmacyId",
                table: "CartItems",
                columns: new[] { "DrugId", "PharmacyId" },
                principalTable: "PharmacyStock",
                principalColumns: new[] { "PharmacyId", "DrugId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_PharmacyStock_DrugId_PharmacyId",
                table: "OrderDetail",
                columns: new[] { "DrugId", "PharmacyId" },
                principalTable: "PharmacyStock",
                principalColumns: new[] { "PharmacyId", "DrugId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_PharmacyStock_DrugId_PharmacyId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_PharmacyStock_DrugId_PharmacyId",
                table: "OrderDetail");

            migrationBuilder.DropTable(
                name: "PharmacyStock");

            migrationBuilder.CreateTable(
                name: "PharmacyStocks",
                columns: table => new
                {
                    PharmacyId = table.Column<int>(type: "int", nullable: false),
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuantityAvailable = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyStocks", x => new { x.PharmacyId, x.DrugId });
                    table.ForeignKey(
                        name: "FK_PharmacyStocks_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "DrugID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PharmacyStocks_Pharmacies_PharmacyId",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacies",
                        principalColumn: "PharmacyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyStocks_DrugId",
                table: "PharmacyStocks",
                column: "DrugId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_PharmacyStocks_DrugId_PharmacyId",
                table: "CartItems",
                columns: new[] { "DrugId", "PharmacyId" },
                principalTable: "PharmacyStocks",
                principalColumns: new[] { "PharmacyId", "DrugId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_PharmacyStocks_DrugId_PharmacyId",
                table: "OrderDetail",
                columns: new[] { "DrugId", "PharmacyId" },
                principalTable: "PharmacyStocks",
                principalColumns: new[] { "PharmacyId", "DrugId" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
