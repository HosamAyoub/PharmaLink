using AutoMapper;
using PharmaLink_API.Models.DTO.AccountDTO;

namespace PharmaLink_API.Models.Profiles
{
    public class RegisterAccountProfile : Profile
    {
        public RegisterAccountProfile()
        {
            CreateMap<RegisterUserDTO, User>();
            CreateMap<RegsiterPharmacyDTO, Pharmacy>();
            CreateMap<RegisterAccountDTO, Account>();
        }
    }
}