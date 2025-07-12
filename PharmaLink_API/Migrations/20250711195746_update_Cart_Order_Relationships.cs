using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PharmaLink_API.Migrations
{
    /// <inheritdoc />
    public partial class update_Cart_Order_Relationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCarts_Drugs_DrugId",
                table: "UserCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCarts_Users_UserId",
                table: "UserCarts");

            migrationBuilder.DropTable(
                name: "OrderDrugs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserCarts",
                table: "UserCarts");

            migrationBuilder.DropIndex(
                name: "IX_UserCarts_DrugId",
                table: "UserCarts");

            migrationBuilder.DeleteData(
                table: "UserCarts",
                keyColumns: new[] { "DrugId", "UserId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "UserCarts",
                keyColumns: new[] { "DrugId", "UserId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.AddColumn<int>(
                name: "PharmacyId",
                table: "UserCarts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserCarts",
                table: "UserCarts",
                columns: new[] { "UserId", "DrugId", "PharmacyId" });

            migrationBuilder.CreateTable(
                name: "Order_PharmacyDrug",
                columns: table => new
                {
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    PharmacyId = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_PharmacyDrug", x => new { x.DrugId, x.PharmacyId, x.OrderID });
                    table.ForeignKey(
                        name: "FK_Order_PharmacyDrug_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_PharmacyDrug_PharmacyDrugs_DrugId_PharmacyId",
                        columns: x => new { x.DrugId, x.PharmacyId },
                        principalTable: "PharmacyDrugs",
                        principalColumns: new[] { "PharmacyId", "DrugId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCarts_DrugId_PharmacyId",
                table: "UserCarts",
                columns: new[] { "DrugId", "PharmacyId" });

            migrationBuilder.CreateIndex(
                name: "IX_Order_PharmacyDrug_OrderID",
                table: "Order_PharmacyDrug",
                column: "OrderID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCarts_PharmacyDrugs_DrugId_PharmacyId",
                table: "UserCarts",
                columns: new[] { "DrugId", "PharmacyId" },
                principalTable: "PharmacyDrugs",
                principalColumns: new[] { "PharmacyId", "DrugId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCarts_Users_UserId",
                table: "UserCarts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCarts_PharmacyDrugs_DrugId_PharmacyId",
                table: "UserCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCarts_Users_UserId",
                table: "UserCarts");

            migrationBuilder.DropTable(
                name: "Order_PharmacyDrug");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserCarts",
                table: "UserCarts");

            migrationBuilder.DropIndex(
                name: "IX_UserCarts_DrugId_PharmacyId",
                table: "UserCarts");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "UserCarts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserCarts",
                table: "UserCarts",
                columns: new[] { "UserId", "DrugId" });

            migrationBuilder.CreateTable(
                name: "OrderDrugs",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDrugs", x => new { x.OrderId, x.DrugId });
                    table.ForeignKey(
                        name: "FK_OrderDrugs_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "DrugID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDrugs_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "OrderDrugs",
                columns: new[] { "DrugId", "OrderId", "Price", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, 50.00m, 2 },
                    { 2, 1, 75.50m, 1 }
                });

            migrationBuilder.InsertData(
                table: "UserCarts",
                columns: new[] { "DrugId", "UserId", "Price", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, 0m, 1 },
                    { 2, 1, 0m, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCarts_DrugId",
                table: "UserCarts",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDrugs_DrugId",
                table: "OrderDrugs",
                column: "DrugId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCarts_Drugs_DrugId",
                table: "UserCarts",
                column: "DrugId",
                principalTable: "Drugs",
                principalColumn: "DrugID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCarts_Users_UserId",
                table: "UserCarts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
