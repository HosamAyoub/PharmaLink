using AutoMapper;
using PharmaLink_API.Models.DTO.PatientDTO;

namespace PharmaLink_API.Models.Profiles
{
    public class PatientProfile : Profile
    {
        public PatientProfile()
        {
            //CreateMap<Patient, PatientDTO>();
            CreateMap<Patient, PatientDTO>().ReverseMap();
            CreateMap<Patient, PatientMedicalInfoDTO>().ReverseMap();
        }
    }
}
