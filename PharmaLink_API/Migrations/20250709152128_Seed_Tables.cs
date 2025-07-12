using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PharmaLink_API.Migrations
{
    /// <inheritdoc />
    public partial class Seed_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "AccountID", "Email", "Password", "Role" },
                values: new object[,]
                {
                    { 1, "mariem@example.com", "hashed_password", "User" },
                    { 2, "elezaby@pharmacy.com", "hashed_password", "Pharmacy" }
                });

            migrationBuilder.InsertData(
                table: "Drugs",
                columns: new[] { "DrugID", "ActiveIngredient", "Category", "CommonName", "Contraindications", "Description", "DosageAndAdministration", "DosageFormsAndStrengths", "DrugInteractions", "IndicationsAndUsage", "SideEffects", "StorageAndHandling", "UNII", "WarningsAndCautions" },
                values: new object[,]
                {
                    { 1, "Paracetamol", "Analgesic", "Panadol", "Severe liver disease", "A common over-the-counter pain reliever.", "Take 1 tablet every 6 hours as needed.", "Tablet: 500mg", "May interact with alcohol and blood thinners.", "Used to relieve pain and reduce fever.", "Nausea, Drowsiness", "Store in a cool, dry place below 30°C.", "Y43GF64R34", "Do not exceed 4g per day." },
                    { 2, "Amoxicillin and Clavulanic acid", "Antibiotic", "Augmentin", "Hypersensitivity to penicillins or clavulanate", "Augmentin is a combination antibiotic containing amoxicillin and clavulanate potassium.", "500mg/125mg tablet every 8 hours or as directed by the physician.", "Tablets: 500mg/125mg, 875mg/125mg", "May interact with methotrexate, anticoagulants, and oral contraceptives.", "Used to treat bacterial infections such as respiratory tract infections, urinary tract infections, and skin infections.", "Diarrhea, Nausea, Rash", "Store below 25°C, protect from moisture.", "25E79B5CTM", "Use with caution in patients with liver impairment or kidney disease." }
                });

            migrationBuilder.InsertData(
                table: "DrugAlternatives",
                columns: new[] { "AlternativeDrugId", "DrugId" },
                values: new object[] { 2, 1 });

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
                table: "UserCarts",
                columns: new[] { "DrugId", "UserId", "Price", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, 0m, 1 },
                    { 2, 1, 0m, 2 }
                });

            migrationBuilder.InsertData(
                table: "UserFavoriteDrugs",
                columns: new[] { "DrugId", "UserId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 }
                });

            migrationBuilder.InsertData(
                table: "OrderDrugs",
                columns: new[] { "DrugId", "OrderId", "Price", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, 50.00m, 2 },
                    { 2, 1, 75.50m, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DrugAlternatives",
                keyColumns: new[] { "AlternativeDrugId", "DrugId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "OrderDrugs",
                keyColumns: new[] { "DrugId", "OrderId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "OrderDrugs",
                keyColumns: new[] { "DrugId", "OrderId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "PharmacyDrugs",
                keyColumns: new[] { "DrugId", "PharmacyId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "PharmacyDrugs",
                keyColumns: new[] { "DrugId", "PharmacyId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "UserCarts",
                keyColumns: new[] { "DrugId", "UserId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "UserCarts",
                keyColumns: new[] { "DrugId", "UserId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "UserFavoriteDrugs",
                keyColumns: new[] { "DrugId", "UserId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "UserFavoriteDrugs",
                keyColumns: new[] { "DrugId", "UserId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "Drugs",
                keyColumn: "DrugID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Drugs",
                keyColumn: "DrugID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderID",
                keyValue: 1);

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
        }
    }
}
