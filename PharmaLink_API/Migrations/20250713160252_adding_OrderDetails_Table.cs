using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaLink_API.Migrations
{
    /// <inheritdoc />
    public partial class adding_OrderDetails_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Orders_OrderID",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_OrderID",
                table: "CartItems");

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderID",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "OrderID",
                table: "CartItems");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderDetail",
                columns: table => new
                {
                    OrderDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    PharmacyId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetail", x => x.OrderDetailId);
                    table.ForeignKey(
                        name: "FK_OrderDetail_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetail_PharmacyStocks_DrugId_PharmacyId",
                        columns: x => new { x.DrugId, x.PharmacyId },
                        principalTable: "PharmacyStocks",
                        principalColumns: new[] { "PharmacyId", "DrugId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_DrugId_PharmacyId",
                table: "OrderDetail",
                columns: new[] { "DrugId", "PharmacyId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_OrderId",
                table: "OrderDetail",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetail");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "OrderID",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CartItems",
                keyColumns: new[] { "DrugId", "PharmacyId", "UserId" },
                keyValues: new object[] { 1, 1, 1 },
                column: "OrderID",
                value: null);

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "OrderID", "Address", "OrderDate", "PaymentMethod", "PharmacyId", "Status", "TotalPrice", "UserId" },
                values: new object[] { 1, "Cairo", new DateTime(2025, 7, 12, 12, 0, 0, 0, DateTimeKind.Unspecified), "Cash", 1, "Pending", 30.00m, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_OrderID",
                table: "CartItems",
                column: "OrderID");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Orders_OrderID",
                table: "CartItems",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "OrderID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
