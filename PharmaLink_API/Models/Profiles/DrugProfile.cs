using AutoMapper;
using PharmaLink_API.Models.DTO.DrugDto;
using PharmaLink_API.Models.DTO.DrugDTO;
using PharmaLink_API.Models.DTO.PharmacyDTO;
using DrugRequestDTO = PharmaLink_API.Models.DTO.DrugDTO.DrugRequestDTO;

namespace PharmaLink_API.Models.Profiles
{
    public class DrugProfile : Profile
    {
        public DrugProfile()
        {
            CreateMap<Drug, DrugDetailsDTO>().ReverseMap();
            CreateMap<Drug, FullPharmaDrugDTO>().ReverseMap();
            CreateMap<Drug, DrugRequestDTO>().ReverseMap();
        }   
    }
}
