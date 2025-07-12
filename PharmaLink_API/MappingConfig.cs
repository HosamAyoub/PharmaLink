using AutoMapper;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.CartDTO;

namespace PharmaLink_API
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<CartItem, AddToCartDTO>().ReverseMap();
        }
    }
}
