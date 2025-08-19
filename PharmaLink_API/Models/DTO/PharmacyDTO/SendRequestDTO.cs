namespace PharmaLink_API.Models.DTO.PharmacyDTO
{
    public class SendRequestDTO
    {
        public string CommonName { get; set; }
        public string ActiveIngredient { get; set; }

        public Status DrugStatus { get; set; } = Status.Pending;

        public int CreatedByPharmacy { get; set; }

    }
}
