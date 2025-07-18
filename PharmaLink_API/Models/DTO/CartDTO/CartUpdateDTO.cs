namespace PharmaLink_API.Models.DTO.CartDTO
{
    public class CartUpdateDTO
    {
        public int UserId { get; set; }
        public int DrugId { get; set; }
        public int PharmacyId { get; set; }
    }
}
