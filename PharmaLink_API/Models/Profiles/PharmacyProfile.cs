using AutoMapper;
using PharmaLink_API.Models.DTO.PharmacyDTO;

namespace PharmaLink_API.Models.Profiles
{
    public class PharmacyProfile : Profile
    {
        public PharmacyProfile()
        {
            CreateMap<Pharmacy, PharmacyDisplayDTO>()
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account!.PhoneNumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account!.Email))
                .ReverseMap();
            CreateMap<Pharmacy, PharmacyUpdateDTO>()
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account!.PhoneNumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account!.Email))
                .ReverseMap();
        }
    }
}
