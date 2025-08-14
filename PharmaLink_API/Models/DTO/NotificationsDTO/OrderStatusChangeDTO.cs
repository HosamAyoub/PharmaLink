namespace PharmaLink_API.Models.DTO.NotificationsDTO
{
    public class OrderStatusChangeDTO
    {
        public int OrderID { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public int PatientId { get; set; }
        public DateTime StatusLastUpdated { get; set; }

    }
}
