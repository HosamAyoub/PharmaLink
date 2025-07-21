using AutoMapper;
using PharmaLink_API.Models.DTO.PharmacyDTO;

namespace PharmaLink_API.Models.Profiles
{
    public class PharmacyProfile : Profile
    {
        public PharmacyProfile()
        {
            CreateMap<Pharmacy, PharmacyDisplayDTO>();
        }
    }
}
