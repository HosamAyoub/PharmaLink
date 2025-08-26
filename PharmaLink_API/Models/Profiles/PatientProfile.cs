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
            CreateMap<Patient, PatientDisplayDTO>()
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account!.PhoneNumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account!.Email))
                .ForMember(dest => dest.userId, opt => opt.MapFrom(src => src.Account!.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Account!.UserName))
                .ForMember (dest => dest.OrderCount, opt => opt.MapFrom(src => src.Orders!.Count));

                
        }
    }
}
