using AutoMapper;
using VNVTStore.Application.DTOs;

using VNVTStore.Domain.Entities;

namespace VNVTStore.Application.MappingProfiles;

/// <summary>
/// AutoMapper Mapping Profile - dùng entities mới với Tbl prefix
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<TblUser, UserDto>().ReverseMap();

        // Product mappings
        CreateMap<TblProduct, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryCodeNavigation != null ? src.CategoryCodeNavigation.Name : null))
            .ReverseMap();
        
        // CreateProductDto -> TblProduct mapping
        CreateMap<Products.Commands.CreateProductDto, TblProduct>();
        
        CreateMap<TblProductImage, ProductImageDto>().ReverseMap();

        // Category mappings
        CreateMap<TblCategory, CategoryDto>().ReverseMap();

        // Order mappings
        CreateMap<TblOrder, OrderDto>()
            .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.TblOrderItems))
            .ReverseMap();
        
        CreateMap<TblOrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductCodeNavigation != null ? src.ProductCodeNavigation.Name : null))
            .ReverseMap();

        // Cart mappings
        CreateMap<TblCart, CartDto>()
            .ForMember(dest => dest.CartItems, opt => opt.MapFrom(src => src.TblCartItems))
            .ReverseMap();
        
        CreateMap<TblCartItem, CartItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductCodeNavigation != null ? src.ProductCodeNavigation.Name : null))
            .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.ProductCodeNavigation != null ? src.ProductCodeNavigation.Price : 0))
            .ReverseMap();

        // Address mappings
        CreateMap<TblAddress, AddressDto>().ReverseMap();

        // Review mappings
        CreateMap<TblReview, ReviewDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserCodeNavigation != null ? src.UserCodeNavigation.Username : null))
            .ReverseMap();

        // Coupon mappings
        CreateMap<TblCoupon, CouponDto>()
            .ForMember(dest => dest.IsValid, opt => opt.Ignore())
            .ReverseMap();

        // Promotion mappings
        CreateMap<TblPromotion, PromotionDto>().ReverseMap();

        // Payment mappings
        CreateMap<TblPayment, PaymentDto>().ReverseMap();

        // Supplier mappings
        CreateMap<TblSupplier, SupplierDto>().ReverseMap();
    }
}
