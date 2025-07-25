using AutoMapper;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.CartDTO;
using PharmaLink_API.Models.DTO.PharmacyStockDTO;

namespace PharmaLink_API
{
    public class MappingConfig: Profile
    {
        public MappingConfig()
        {
            CreateMap<CartItem, AddToCartDTO>().ReverseMap();
            CreateMap<CartItem, CartItemSummaryDTO>().ReverseMap();
            CreateMap<PharmacyProduct, pharmacyProductDTO>().ReverseMap();
            CreateMap<Pharmacy, PharmacyDTO>().ReverseMap();
            
        }
    }
}
    

