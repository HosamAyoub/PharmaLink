namespace PharmaLink_API.Models.DTO.DrugDto
{
    public class DrugDetailsDTO
    {
        public int DrugID { get; set; }
        public string CommonName { get; set; }
        public string Category { get; set; }
        public string ActiveIngredient { get; set; }
        public string? Alternatives_names { get; set; }
        public int? AlternativesGpID { get; set; }
        public string? Indications_and_usage { get; set; }
        public string? Dosage_and_administration { get; set; }
        public string? Dosage_forms_and_strengths { get; set; }
        public string? Contraindications { get; set; }
        public string? Warnings_and_cautions { get; set; }
        public string? Drug_interactions { get; set; }
        public string? Description { get; set; }
        public string? Storage_and_handling { get; set; }
        public string? Adverse_reactions { get; set; }
        public string? Drug_UrlImg { get; set; }
        public Status DrugStatus { get; set; }
        public int? CreatedByPharmacy {get; set; }
    }
}
