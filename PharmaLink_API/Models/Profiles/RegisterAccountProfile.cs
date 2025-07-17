using AutoMapper;
using PharmaLink_API.Models.DTO.RegisterAccountDTO;

namespace PharmaLink_API.Models.Profiles
{
    public class RegisterAccountProfile : Profile
    {
        public RegisterAccountProfile()
        {
            CreateMap<RegisterPatientDTO, Patient>();
            CreateMap<RegisterPatientDTO, Patient>();
            CreateMap<RegsiterPharmacyDTO, Pharmacy>();
            CreateMap<RegisterAccountDTO, Account>();
        }
    }
}