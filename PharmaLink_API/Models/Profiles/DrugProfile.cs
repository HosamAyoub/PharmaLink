using AutoMapper;
using PharmaLink_API.Models.DTO.DrugDto;
using PharmaLink_API.Models.DTO.DrugDTO;

namespace PharmaLink_API.Models.Profiles
{
    public class DrugProfile : Profile
    {
        public DrugProfile()
        {
            CreateMap<Drug, DrugDetailsDTO>().ReverseMap();
            CreateMap<Drug, FullPharmaDrugDTO>().ReverseMap();
        }   
    }
}
