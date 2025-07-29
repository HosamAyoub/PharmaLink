using PharmaLink_API.Models.DTO.DrugDto;

namespace PharmaLink_API.Models.DTO.DrugDTO
{
    public class FullPharmaDrugDTO
    {
        public DrugDetailsDTO Drug_Info { get; set; }

        public List<PharmaDataDTO> Pharma_Info { get; set; }


    }

    public class PharmaDataDTO
    {
        public int Pharma_Id { get; set; }
        public string Pharma_Name { get; set; }
        public string Pharma_Address { get; set; }
        public string Pharma_Location { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }


    }
}
