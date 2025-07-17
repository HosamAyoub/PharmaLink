namespace PharmaLink_API.Models
{
    public class PatientFavoriteDrug
    {
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int DrugId { get; set; }
        public Drug? Drug { get; set; }
    }
}
