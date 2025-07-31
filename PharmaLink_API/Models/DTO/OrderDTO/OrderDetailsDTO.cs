namespace PharmaLink_API.Models.DTO.OrderDTO
{
    public class OrderDetailsDTO
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }

        // Patient
        public string PatientName { get; set; }
        public string PatientPhone { get; set; }
        public string PatientAddress { get; set; }

        // Payment
        public string PaymentMethod { get; set; }
        public decimal Total { get; set; }

        // Medicines
        public List<string> Medicines { get; set; }

        // Status
        public string CurrentStatus { get; set; }
        public int EstimatedCompletionMinutes { get; set; }
    }
}
