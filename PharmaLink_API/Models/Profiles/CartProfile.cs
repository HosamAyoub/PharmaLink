using AutoMapper;
using PharmaLink_API.Models.DTO.CartDTO;
using PharmaLink_API.Models.DTO.OrderDTO;

namespace PharmaLink_API.Models.Profiles
{
    public class CartProfile : Profile
    {
        public CartProfile()
        {
            CreateMap<CartItem, AddToCartDTO>().ReverseMap();
            CreateMap<CartItem, CartItemSummaryDTO>().ReverseMap();
            CreateMap<CartItem, CartItemResponseDTO>().ReverseMap();
            CreateMap<CartItem, CartItemDetailsDTO>()
                .ForMember(dest => dest.DrugName, opt => opt.MapFrom(src => src.PharmacyProduct != null && src.PharmacyProduct.Drug != null? src.PharmacyProduct.Drug.CommonName : "Unknown Drug"))
                .ForMember(dest => dest.PharmacyName, opt => opt.MapFrom(src => src.PharmacyProduct != null && src.PharmacyProduct.Pharmacy != null? src.PharmacyProduct.Pharmacy.Name : "Unknown Pharmacy"))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.PharmacyProduct != null && src.PharmacyProduct.Drug != null? src.PharmacyProduct.Drug.Drug_UrlImg : ""))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Price));

            CreateMap<Patient, OrderSummaryDTO>()
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account!.PhoneNumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account!.Email))
                .ForMember(dest => dest.Subtotal, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryFee, opt => opt.Ignore());

            CreateMap<Order, PharmacyOrderDTO>();
            CreateMap<OrderDetail, OrderItemDTO>();
        }
    }
}