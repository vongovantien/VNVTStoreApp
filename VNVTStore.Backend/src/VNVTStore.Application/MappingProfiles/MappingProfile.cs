using AutoMapper;
using VNVTStore.Application.DTOs;

using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.MappingProfiles;

using VNVTStore.Application.Mappings.Resolvers;

/// <summary>
/// AutoMapper Mapping Profile - dùng entities mới với Tbl prefix
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<TblUser, UserDto>()
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.AvatarUrl)); // Map AvatarUrl to Avatar


        // Banner mappings
        CreateMap<TblBanner, BannerDto>().ReverseMap();
        CreateMap<CreateBannerDto, TblBanner>();
        CreateMap<UpdateBannerDto, TblBanner>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Brand mappings
        CreateMap<TblBrand, BrandDto>().ReverseMap();
        CreateMap<CreateBrandDto, TblBrand>();
        CreateMap<UpdateBrandDto, TblBrand>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Product mappings
        CreateMap<TblProduct, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryCodeNavigation != null ? src.CategoryCodeNavigation.Name : null))
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.TblProductDetails))
            .ForMember(dest => dest.ProductUnits, opt => opt.MapFrom(src => src.TblProductUnits))
            .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
            .ReverseMap()
            .ForMember(dest => dest.CategoryCodeNavigation, opt => opt.Ignore())
            .ForMember(dest => dest.Brand, opt => opt.Ignore());
            
        CreateMap<TblProductDetail, ProductDetailDto>().ReverseMap();
        
        // CreateProductDto -> TblProduct mapping
        CreateMap<CreateProductDto, TblProduct>()
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.TblProductDetails, opt => opt.MapFrom(src => src.Details))
            .ForMember(dest => dest.TblProductUnits, opt => opt.Ignore()); // Handled manually in Handler
        
        CreateMap<UpdateProductDto, TblProduct>()
            .ForMember(dest => dest.TblProductDetails, opt => opt.MapFrom(src => src.Details))
            .ForMember(dest => dest.TblProductUnits, opt => opt.Ignore()) // Handled manually
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // CreateMap<CreateUnitDto, TblUnit>() ... Removed, we use TblProductUnit 

        CreateMap<TblUnit, CatalogUnitDto>().ReverseMap();
        CreateMap<CreateCatalogUnitDto, TblUnit>();
        CreateMap<UpdateCatalogUnitDto, TblUnit>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<TblProductUnit, UnitDto>()
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null))
            .ReverseMap();

        
        // CreateMap<TblProductImage, ProductImageDto>().ReverseMap(); // Removed

        CreateMap<TblCategory, CategoryDto>()
            .ForMember(dest => dest.ImageURL, opt => opt.MapFrom<ImageUrlResolver, string?>(src => src.ImageURL))
            .ReverseMap();

        // Order mappings
        CreateMap<TblOrder, OrderDto>()
            .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.TblOrderItems));
        
        CreateMap<TblOrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductCodeNavigation != null ? src.ProductCodeNavigation.Name : null));

        // Cart mappings
        CreateMap<TblCart, CartDto>()
            .ForMember(dest => dest.CartItems, opt => opt.MapFrom(src => src.TblCartItems))
            .ReverseMap();
        
        CreateMap<TblCartItem, CartItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductCodeNavigation != null ? src.ProductCodeNavigation.Name : null))
            .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.ProductCodeNavigation != null ? src.ProductCodeNavigation.Price : 0))
            .ReverseMap();

        // Address mappings
        CreateMap<TblAddress, AddressDto>();

        // Review mappings
        CreateMap<TblReview, ReviewDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserCodeNavigation != null ? src.UserCodeNavigation.Username : null))
            .ReverseMap();

        // Coupon mappings
        CreateMap<TblCoupon, CouponDto>()
            .ForMember(dest => dest.IsValid, opt => opt.Ignore())
            .ReverseMap();

        // Promotion mappings


        // Payment mappings
        CreateMap<TblPayment, PaymentDto>();

        // Supplier mappings
        CreateMap<TblSupplier, SupplierDto>().ReverseMap();

        // Quote mappings
        // Quote mappings
    CreateMap<TblQuote, QuoteDto>()
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductCodeNavigation != null ? src.ProductCodeNavigation.Name : null))
        .ForMember(dest => dest.ProductImage, opt => opt.Ignore())
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

        // Promotion mappings
        CreateMap<TblPromotion, PromotionDto>()
            .ForMember(d => d.ProductCodes, o => o.MapFrom(s => s.TblProductPromotions.Select(pp => pp.ProductCode).ToList()))
            .ReverseMap();

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
