using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PharmaLink_API.Migrations
{
    /// <inheritdoc />
    public partial class Updating_Drug_Relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrugAlternatives");

            migrationBuilder.DropIndex(
                name: "IX_Drugs_UNII",
                table: "Drugs");

            migrationBuilder.DeleteData(
                table: "Drugs",
                keyColumn: "DrugID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Drugs",
                keyColumn: "DrugID",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "UNII",
                table: "Drugs");

            migrationBuilder.RenameColumn(
                name: "WarningsAndCautions",
                table: "Drugs",
                newName: "Warnings_and_cautions");

            migrationBuilder.RenameColumn(
                name: "StorageAndHandling",
                table: "Drugs",
                newName: "Storage_and_handling");

            migrationBuilder.RenameColumn(
                name: "SideEffects",
                table: "Drugs",
                newName: "Indications_and_usage");

            migrationBuilder.RenameColumn(
                name: "IndicationsAndUsage",
                table: "Drugs",
                newName: "Drug_interactions");

            migrationBuilder.RenameColumn(
                name: "DrugInteractions",
                table: "Drugs",
                newName: "Drug_UrlImg");

            migrationBuilder.RenameColumn(
                name: "DosageFormsAndStrengths",
                table: "Drugs",
                newName: "Dosage_forms_and_strengths");

            migrationBuilder.RenameColumn(
                name: "DosageAndAdministration",
                table: "Drugs",
                newName: "Dosage_and_administration");

            migrationBuilder.AddColumn<string>(
                name: "Adverse_reactions",
                table: "Drugs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AlternativesGpID",
                table: "Drugs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Alternatives_names",
                table: "Drugs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adverse_reactions",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "AlternativesGpID",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "Alternatives_names",
                table: "Drugs");

            migrationBuilder.RenameColumn(
                name: "Warnings_and_cautions",
                table: "Drugs",
                newName: "WarningsAndCautions");

            migrationBuilder.RenameColumn(
                name: "Storage_and_handling",
                table: "Drugs",
                newName: "StorageAndHandling");

            migrationBuilder.RenameColumn(
                name: "Indications_and_usage",
                table: "Drugs",
                newName: "SideEffects");

            migrationBuilder.RenameColumn(
                name: "Drug_interactions",
                table: "Drugs",
                newName: "IndicationsAndUsage");

            migrationBuilder.RenameColumn(
                name: "Drug_UrlImg",
                table: "Drugs",
                newName: "DrugInteractions");

            migrationBuilder.RenameColumn(
                name: "Dosage_forms_and_strengths",
                table: "Drugs",
                newName: "DosageFormsAndStrengths");

            migrationBuilder.RenameColumn(
                name: "Dosage_and_administration",
                table: "Drugs",
                newName: "DosageAndAdministration");

            migrationBuilder.AddColumn<string>(
                name: "UNII",
                table: "Drugs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DrugAlternatives",
                columns: table => new
                {
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    AlternativeDrugId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugAlternatives", x => new { x.DrugId, x.AlternativeDrugId });
                    table.ForeignKey(
                        name: "FK_DrugAlternatives_Drugs_AlternativeDrugId",
                        column: x => x.AlternativeDrugId,
                        principalTable: "Drugs",
                        principalColumn: "DrugID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DrugAlternatives_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "DrugID",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_UNII",
                table: "Drugs",
                column: "UNII",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrugAlternatives_AlternativeDrugId",
                table: "DrugAlternatives",
                column: "AlternativeDrugId");
        }
    }
}
