namespace PharmaLink_API.Models.DTO.CartDTO
{
    public class SubmitOrderRequestDTO
    {
        public int PatientId { get; set; }
        public List<OrderItemDTO> Items { get; set; }
    }
}
