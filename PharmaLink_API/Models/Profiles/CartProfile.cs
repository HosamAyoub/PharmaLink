using AutoMapper;
using PharmaLink_API.Models.DTO.CartDTO;

namespace PharmaLink_API.Models.Profiles
{
    public class CartProfile : Profile
    {
        public CartProfile()
        {
            CreateMap<CartItem, AddToCartDTO>().ReverseMap();
            CreateMap<CartItem, CartItemSummaryDTO>().ReverseMap();
        }
    }
}