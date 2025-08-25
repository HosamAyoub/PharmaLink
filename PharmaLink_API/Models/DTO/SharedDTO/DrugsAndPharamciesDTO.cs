namespace PharmaLink_API.Models.DTO.SharedDTO
{
    public class DrugsAndPharamciesDTO
    {
        public List<DrugSearchDTO>? drugs { get; set; }
        public List<PharmacySearchDTO>? pharmacies { get; set; }
    }

    public class DrugSearchDTO 
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PharmacySearchDTO 
    {
        public int Id { get; set; }
        public string Name {get; set; }
    }
}
