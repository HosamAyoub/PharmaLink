namespace PharmaLink_API.Models
{
    public class Drug
    {
        public int DrugID { get; set; }
        public string? CommonName { get; set; }
        public string? Category { get; set; }
        public string? ActiveIngredient { get; set; }
        public string? Alternatives_names { get; set; }
        public int AlternativesGpID { get; set; }
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

<<<<<<< HEAD
        public Status DrugStatus { get; set; } 
=======
        public Status? DrugStatus { get; set; } = Status.Approved;
>>>>>>> 0d9b80bb98abe82c49ad8c686f8e96d142b82201

        public int? CreatedByPharmacy { get; set; }

        public Boolean IsRead { get; set; } = false;
        

        public DateTime? CreatedAt { get; set; } = DateTime.Now;




        //Drug-Pharmacy relationship (many to many)
        public ICollection<PharmacyProduct>? PharmacyStock { get; set; }

        //Drug-Patient(favorites) relationship (many to many)
        public ICollection<PatientFavoriteDrug>? PatientFavorites { get; set; }
    }

    public enum Status
    {
        Rejected=0,
        Approved=1,
        Pending=2
    }
}
