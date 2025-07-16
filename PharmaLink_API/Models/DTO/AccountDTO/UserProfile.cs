using AutoMapper;

namespace PharmaLink_API.Models.DTO.AccountDTO
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDTO, User>();
            CreateMap<PharmacyDTO, Pharmacy>();
            CreateMap<AccountDTO, Account>();
        }
    }
}