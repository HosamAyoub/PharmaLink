using AutoMapper;
using Microsoft.AspNetCore.Identity;
using PharmaLink_API.Models.DTO.RolesDTO;

namespace PharmaLink_API.Models.Profiles
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<RoleDTO, IdentityRole>();
        }
    }
}
