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

        // Banner mappings
        CreateMap<TblBanner, BannerDto>().ReverseMap();
        CreateMap<CreateBannerDto, TblBanner>();
        CreateMap<UpdateBannerDto, TblBanner>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Product mappings
        CreateMap<TblProduct, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryCodeNavigation != null ? src.CategoryCodeNavigation.Name : null))
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => (string?)null))
            .ForMember(dest => dest.IsNew, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IsFeatured, opt => opt.MapFrom(src => false))
            .ReverseMap()
            .ForMember(dest => dest.CategoryCodeNavigation, opt => opt.Ignore());
        
        // CreateProductDto -> TblProduct mapping
        CreateMap<CreateProductDto, TblProduct>()
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierCode, opt => opt.Ignore());
        
        CreateMap<UpdateProductDto, TblProduct>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        
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

        // Quote mappings
        // Quote mappings
    CreateMap<TblQuote, QuoteDto>()
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductCodeNavigation != null ? src.ProductCodeNavigation.Name : null))
        .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.ProductCodeNavigation != null && src.ProductCodeNavigation.TblProductImages.Any(i => i.IsPrimary == true) 
            ? src.ProductCodeNavigation.TblProductImages.First(i => i.IsPrimary == true).ImageUrl 
            : (src.ProductCodeNavigation != null && src.ProductCodeNavigation.TblProductImages.Any() ? src.ProductCodeNavigation.TblProductImages.First().ImageUrl : null)))
        .ReverseMap();

    CreateMap<CreateQuoteDto, TblQuote>();
    CreateMap<UpdateQuoteDto, TblQuote>()
         .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

    // Category Create/Update
    CreateMap<CreateCategoryDto, TblCategory>();
    CreateMap<UpdateCategoryDto, TblCategory>()
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

    // Address Create/Update
    CreateMap<CreateAddressDto, TblAddress>();
    CreateMap<UpdateAddressDto, TblAddress>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

    // Review Create/Update
    CreateMap<CreateReviewDto, TblReview>();
    CreateMap<UpdateReviewDto, TblReview>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

    // Promotion Create/Update
    CreateMap<CreatePromotionDto, TblPromotion>();
    CreateMap<UpdatePromotionDto, TblPromotion>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
    // Supplier Create/Update
    CreateMap<CreateSupplierDto, TblSupplier>();
    CreateMap<UpdateSupplierDto, TblSupplier>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
