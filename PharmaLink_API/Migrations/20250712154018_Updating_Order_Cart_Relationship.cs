using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PharmaLink_API.Migrations
{
    /// <inheritdoc />
    public partial class Updating_Order_Cart_Relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Order_PharmacyDrug");

            migrationBuilder.DropTable(
                name: "UserCarts");

            migrationBuilder.DropTable(
                name: "PharmacyDrugs");

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UserFavoriteDrugs",
                keyColumns: new[] { "DrugId", "UserId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "UserFavoriteDrugs",
                keyColumns: new[] { "DrugId", "UserId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "Pharmacies",
                keyColumn: "PharmacyID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "AccountID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "AccountID",
                keyValue: 2);

            migrationBuilder.CreateTable(
                name: "PharmacyStocks",
                columns: table => new
                {
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    PharmacyId = table.Column<int>(type: "int", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    PharmacyId = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => new { x.UserId, x.DrugId, x.PharmacyId });
                    table.ForeignKey(
                        name: "FK_CartItems_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_PharmacyStocks_DrugId_PharmacyId",
                        columns: x => new { x.DrugId, x.PharmacyId },
                        principalTable: "PharmacyStocks",
                        principalColumns: new[] { "PharmacyId", "DrugId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CartItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_DrugId_PharmacyId",
                table: "CartItems",
                columns: new[] { "DrugId", "PharmacyId" });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_OrderID",
                table: "CartItems",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyStocks_DrugId",
                table: "PharmacyStocks",
                column: "DrugId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "PharmacyStocks");

            migrationBuilder.CreateTable(
                name: "PharmacyDrugs",
                columns: table => new
                {
                    PharmacyId = table.Column<int>(type: "int", nullable: false),
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuantityAvailable = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyDrugs", x => new { x.PharmacyId, x.DrugId });
                    table.ForeignKey(
                        name: "FK_PharmacyDrugs_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "DrugID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PharmacyDrugs_Pharmacies_PharmacyId",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacies",
                        principalColumn: "PharmacyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Order_PharmacyDrug",
                columns: table => new
                {
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    PharmacyId = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "UserCarts",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    PharmacyId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCarts", x => new { x.UserId, x.DrugId, x.PharmacyId });
                    table.ForeignKey(
                        name: "FK_UserCarts_PharmacyDrugs_DrugId_PharmacyId",
                        columns: x => new { x.DrugId, x.PharmacyId },
                        principalTable: "PharmacyDrugs",
                        principalColumns: new[] { "PharmacyId", "DrugId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCarts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "AccountID", "Email", "Password", "Role" },
                values: new object[,]
                {
                    { 1, "mariem@example.com", "hashed_password", "User" },
                    { 2, "elezaby@pharmacy.com", "hashed_password", "Pharmacy" }
                });

            migrationBuilder.InsertData(
                table: "Pharmacies",
                columns: new[] { "PharmacyID", "AccountId", "Address", "Country", "EndHour", "Name", "Rate", "StartHour" },
                values: new object[] { 1, 2, "12 Tahrir St, Cairo", "Egypt", new TimeOnly(23, 0, 0), "El Ezaby Pharmacy", 4.7999999999999998, new TimeOnly(9, 0, 0) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "AccountId", "Address", "Country", "DateOfBirth", "Gender", "MobileNumber", "Name", "UserDisease", "UserDrugs" },
                values: new object[] { 1, 1, "123 Nile Street, Cairo", "Egypt", new DateTime(1999, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Female", "01012345678", "Mariem Sameh", "Diabetes", "Metformin, Insulin" });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "OrderID", "Address", "OrderDate", "PaymentMethod", "PharmacyId", "Status", "TotalPrice", "UserId" },
                values: new object[] { 1, "456 Zamalek St, Cairo", new DateTime(2025, 7, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cash", 1, "Pending", 175.50m, 1 });

            migrationBuilder.InsertData(
                table: "PharmacyDrugs",
                columns: new[] { "DrugId", "PharmacyId", "Price", "QuantityAvailable" },
                values: new object[,]
                {
                    { 1, 1, 50.00m, 100 },
                    { 2, 1, 75.50m, 50 }
                });

            migrationBuilder.InsertData(
                table: "UserFavoriteDrugs",
                columns: new[] { "DrugId", "UserId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Order_PharmacyDrug_OrderID",
                table: "Order_PharmacyDrug",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDrugs_DrugId",
                table: "PharmacyDrugs",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCarts_DrugId_PharmacyId",
                table: "UserCarts",
                columns: new[] { "DrugId", "PharmacyId" });
        }
    }
}
