using PharmaLink_API.Core.Enums;

namespace PharmaLink_API.Models.DTO.PharmacyDTO
{
    public class PharmacyWithDistanceDTO
    {
        public int pharma_Id { get; set; }
        public string pharma_Name { get; set; }
        public string pharma_Address { get; set; }
        public double pharma_Latitude { get; set; }
        public double pharma_Longitude { get; set; }
        public decimal price { get; set; }
        public int quantityAvailable { get; set; }
        public double Distance { get; set; }
    }
}
