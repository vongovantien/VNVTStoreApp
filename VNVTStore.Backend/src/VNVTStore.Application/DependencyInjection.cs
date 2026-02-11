using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using MediatR;
using FluentValidation;
using VNVTStore.Application.Common;
using VNVTStore.Application.Common.Behaviors;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.MappingProfiles;
using VNVTStore.Application.Strategies;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Strategies;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Products.Handlers;

namespace VNVTStore.Application;

/// <summary>
/// Dependency Injection cho Application Layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // Add FluentValidation - auto register all validators in assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Add MediatR - tự động register tất cả handlers trong Assembly
        // Exclude GenericHandler from auto-registration because we register it manually below
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            
            // Register ValidationBehavior pipeline
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            cfg.TypeEvaluator = type =>
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BaseHandler<,,,>))
                    return false;
                return true;
            };
        });

        // Registry Strategies
        services.AddSingleton<IShippingStrategy, StandardShippingStrategy>();

        // Register Data Seeder


        // Register Generic Handlers for entities
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblBrand, BrandDto, CreateBrandDto, UpdateBrandDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblSupplier, SupplierDto, CreateSupplierDto, UpdateSupplierDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblCategory, CategoryDto, CreateCategoryDto, UpdateCategoryDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblBanner, BannerDto, CreateBannerDto, UpdateBannerDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblUnit, CatalogUnitDto, CreateCatalogUnitDto, UpdateCatalogUnitDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblTag, TagDto, CreateTagDto, UpdateTagDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblPromotion, PromotionDto, CreatePromotionDto, UpdatePromotionDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblProduct, ProductDto, CreateProductDto, UpdateProductDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblOrder, OrderDto, CreateOrderDto, UpdateOrderDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblUser, UserDto, CreateUserDto, UpdateUserDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblAddress, AddressDto, CreateAddressDto, UpdateAddressDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblReview, ReviewDto, CreateReviewDto, UpdateReviewDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblQuote, QuoteDto, CreateQuoteDto, UpdateQuoteDto>();
        
        // Specialized Handlers (Override generic registrations)
        services.AddScoped<IRequestHandler<GetPagedQuery<ProductDto>, Result<PagedResult<ProductDto>>>, GetProductsHandler>();
        services.AddScoped<IRequestHandler<CreateCommand<CreateProductDto, ProductDto>, Result<ProductDto>>, CreateProductHandler>();
        services.AddScoped<IRequestHandler<UpdateCommand<UpdateProductDto, ProductDto>, Result<ProductDto>>, UpdateProductHandler>();
        
        // Banner specialized handlers
        services.AddScoped<IRequestHandler<GetPagedQuery<BannerDto>, Result<PagedResult<BannerDto>>>, VNVTStore.Application.Banners.Handlers.GetBannersHandler>();
        services.AddScoped<IRequestHandler<GetByCodeQuery<BannerDto>, Result<BannerDto>>, VNVTStore.Application.Banners.Handlers.GetBannerByCodeHandler>();
        services.AddScoped<IRequestHandler<CreateCommand<CreateBannerDto, BannerDto>, Result<BannerDto>>, VNVTStore.Application.Banners.Handlers.CreateBannerHandler>();
        services.AddScoped<IRequestHandler<UpdateCommand<UpdateBannerDto, BannerDto>, Result<BannerDto>>, VNVTStore.Application.Banners.Handlers.UpdateBannerHandler>();

        return services;
    }

    private static IServiceCollection AddGenericHandler<TEntity, TResponse, TCreateDto, TUpdateDto>(this IServiceCollection services)
        where TEntity : class, IEntity
        where TResponse : class, IBaseDto, new()
        where TCreateDto : class
        where TUpdateDto : class
    {
        var handlerType = typeof(BaseHandler<TEntity, TResponse, TCreateDto, TUpdateDto>);
        
        services.AddTransient(typeof(IRequestHandler<GetPagedQuery<TResponse>, Result<PagedResult<TResponse>>>), handlerType);
        services.AddTransient(typeof(IRequestHandler<GetByCodeQuery<TResponse>, Result<TResponse>>), handlerType);
        services.AddTransient(typeof(IRequestHandler<CreateCommand<TCreateDto, TResponse>, Result<TResponse>>), handlerType);
        services.AddTransient(typeof(IRequestHandler<UpdateCommand<TUpdateDto, TResponse>, Result<TResponse>>), handlerType);
        services.AddTransient(typeof(IRequestHandler<DeleteCommand<TEntity>, Result>), handlerType);
        services.AddTransient(typeof(IRequestHandler<DeleteMultipleCommand<TEntity>, Result>), handlerType);
        services.AddTransient(typeof(IRequestHandler<GetStatsQuery<TEntity>, Result<EntityStatsDto>>), handlerType);

        return services;
    }



}


