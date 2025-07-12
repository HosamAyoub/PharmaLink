using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PharmaLink_API.Migrations
{
    /// <inheritdoc />
    public partial class Seed_Tables_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "AccountID", "Email", "Password", "Role" },
                values: new object[,]
                {
                    { 1, "user1@example.com", "hashedpass", "User" },
                    { 2, "pharmacy@example.com", "hashedpass", "Pharmacy" }
                });

            migrationBuilder.InsertData(
                table: "Drugs",
                columns: new[] { "DrugID", "ActiveIngredient", "Adverse_reactions", "AlternativesGpID", "Alternatives_names", "Category", "CommonName", "Contraindications", "Description", "Dosage_and_administration", "Dosage_forms_and_strengths", "Drug_UrlImg", "Drug_interactions", "Indications_and_usage", "Storage_and_handling", "Warnings_and_cautions" },
                values: new object[] { 1, "Paracetamol", "Nausea", 100, "Tylenol", "Painkiller", "Panadol", "Liver disease", "Pain reliever", "500mg twice daily", "Tablet 500mg", "/images/panadol.png", "Warfarin", "Headache, fever", "Keep cool and dry", "Don't exceed 4g/day" });

            migrationBuilder.InsertData(
                table: "Pharmacies",
                columns: new[] { "PharmacyID", "AccountId", "Address", "Country", "EndHour", "Name", "Rate", "StartHour" },
                values: new object[] { 1, 2, "Nasr City", "Egypt", new TimeOnly(0, 0, 0), "Good Health", 0.0, new TimeOnly(0, 0, 0) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "AccountId", "Address", "Country", "DateOfBirth", "Gender", "MobileNumber", "Name", "UserDisease", "UserDrugs" },
                values: new object[] { 1, 1, "Cairo", "Egypt", new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Female", "0100000000", "Mariem", "None", "Paracetamol" });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "OrderID", "Address", "OrderDate", "PaymentMethod", "PharmacyId", "Status", "TotalPrice", "UserId" },
                values: new object[] { 1, "Cairo", new DateTime(2025, 7, 12, 12, 0, 0, 0, DateTimeKind.Unspecified), "Cash", 1, "Pending", 30.00m, 1 });

            migrationBuilder.InsertData(
                table: "PharmacyStocks",
                columns: new[] { "DrugId", "PharmacyId", "Price", "QuantityAvailable" },
                values: new object[] { 1, 1, 15.00m, 50 });

            migrationBuilder.InsertData(
                table: "UserFavoriteDrugs",
                columns: new[] { "DrugId", "UserId" },
                values: new object[] { 1, 1 });

            migrationBuilder.InsertData(
                table: "CartItems",
                columns: new[] { "DrugId", "PharmacyId", "UserId", "OrderID", "Price", "Quantity" },
                values: new object[] { 1, 1, 1, null, 0m, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumns: new[] { "DrugId", "PharmacyId", "UserId" },
                keyValues: new object[] { 1, 1, 1 });

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UserFavoriteDrugs",
                keyColumns: new[] { "DrugId", "UserId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "PharmacyStocks",
                keyColumns: new[] { "DrugId", "PharmacyId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "AccountID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Drugs",
                keyColumn: "DrugID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Pharmacies",
                keyColumn: "PharmacyID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "AccountID",
                keyValue: 2);
        }
    }
}
