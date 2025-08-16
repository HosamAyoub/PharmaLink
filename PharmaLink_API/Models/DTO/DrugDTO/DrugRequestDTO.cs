using PharmaLink_API.Models.DTO.DrugDto;

namespace PharmaLink_API.Models.DTO.DrugDTO
{
    public class DrugRequestDTO
    {
        public Pharmacy_Sender Pharmacy { get; set; }
        public DrugDetailsDTO DrugRequests { get; set; }
    }


    public class Pharmacy_Sender
    {
        public int Pharmacy_Id { get; set; }
        public string Pharmacy_Name { get; set; }

    }
}