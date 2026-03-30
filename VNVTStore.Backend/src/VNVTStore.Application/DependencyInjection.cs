using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using MediatR;
using FluentValidation;
using VNVTStore.Application.Common;
using VNVTStore.Application.Common.Behaviors;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Services;
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
            
            // Register AuditLoggingBehavior pipeline
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuditLoggingBehavior<,>));

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
        // services.AddGenericHandler<VNVTStore.Domain.Entities.TblBrand, BrandDto, CreateBrandDto, UpdateBrandDto>();
        // services.AddGenericHandler<VNVTStore.Domain.Entities.TblSupplier, SupplierDto, CreateSupplierDto, UpdateSupplierDto>();
        // services.AddGenericHandler<VNVTStore.Domain.Entities.TblCategory, CategoryDto, CreateCategoryDto, UpdateCategoryDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblPermission, PermissionDto, PermissionDto, PermissionDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblMenu, MenuDto, MenuDto, MenuDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblUnit, CatalogUnitDto, CreateCatalogUnitDto, UpdateCatalogUnitDto>();
        services.AddGenericHandler<VNVTStore.Domain.Entities.TblTag, TagDto, CreateTagDto, UpdateTagDto>();
        // Address has explicit handler: services.AddGenericHandler<VNVTStore.Domain.Entities.TblAddress, AddressDto, CreateAddressDto, UpdateAddressDto>();

        
        // Specialized Handlers (Override generic registrations)
        services.AddScoped<IRequestHandler<GetPagedQuery<ProductDto>, Result<PagedResult<ProductDto>>>, GetProductsHandler>();
        services.AddScoped<IRequestHandler<CreateCommand<CreateProductDto, ProductDto>, Result<ProductDto>>, CreateProductHandler>();
        services.AddScoped<IRequestHandler<UpdateCommand<UpdateProductDto, ProductDto>, Result<ProductDto>>, UpdateProductHandler>();
        
        // Banner specialized handlers
        services.AddScoped<IRequestHandler<GetPagedQuery<BannerDto>, Result<PagedResult<BannerDto>>>, VNVTStore.Application.Banners.Handlers.GetBannersHandler>();
        services.AddScoped<IRequestHandler<GetByCodeQuery<BannerDto>, Result<BannerDto>>, VNVTStore.Application.Banners.Handlers.GetBannerByCodeHandler>();
        services.AddScoped<IRequestHandler<CreateCommand<CreateBannerDto, BannerDto>, Result<BannerDto>>, VNVTStore.Application.Banners.Handlers.CreateBannerHandler>();
        services.AddScoped<IRequestHandler<UpdateCommand<UpdateBannerDto, BannerDto>, Result<BannerDto>>, VNVTStore.Application.Banners.Handlers.UpdateBannerHandler>();

        // Brand Specialized Handlers
        services.AddScoped<IRequestHandler<CreateCommand<CreateBrandDto, BrandDto>, Result<BrandDto>>, VNVTStore.Application.Brands.Handlers.BrandHandlers>();
        services.AddScoped<IRequestHandler<UpdateCommand<UpdateBrandDto, BrandDto>, Result<BrandDto>>, VNVTStore.Application.Brands.Handlers.BrandHandlers>();
        services.AddScoped<IRequestHandler<DeleteCommand<TblBrand>, Result>, VNVTStore.Application.Brands.Handlers.BrandHandlers>();
        services.AddScoped<IRequestHandler<GetByCodeQuery<BrandDto>, Result<BrandDto>>, VNVTStore.Application.Brands.Handlers.BrandHandlers>();
        services.AddScoped<IRequestHandler<GetPagedQuery<BrandDto>, Result<PagedResult<BrandDto>>>, VNVTStore.Application.Brands.Handlers.BrandHandlers>();
        services.AddScoped<IRequestHandler<DeleteMultipleCommand<TblBrand>, Result>, VNVTStore.Application.Brands.Handlers.BrandHandlers>();
        services.AddScoped<IRequestHandler<GetStatsQuery<TblBrand>, Result<EntityStatsDto>>, VNVTStore.Application.Brands.Handlers.BrandHandlers>();

        // Supplier Specialized Handlers
        services.AddScoped<IRequestHandler<CreateCommand<CreateSupplierDto, SupplierDto>, Result<SupplierDto>>, VNVTStore.Application.Suppliers.Handlers.SupplierHandlers>();
        services.AddScoped<IRequestHandler<UpdateCommand<UpdateSupplierDto, SupplierDto>, Result<SupplierDto>>, VNVTStore.Application.Suppliers.Handlers.SupplierHandlers>();
        services.AddScoped<IRequestHandler<DeleteCommand<TblSupplier>, Result>, VNVTStore.Application.Suppliers.Handlers.SupplierHandlers>();
        services.AddScoped<IRequestHandler<GetByCodeQuery<SupplierDto>, Result<SupplierDto>>, VNVTStore.Application.Suppliers.Handlers.SupplierHandlers>();
        services.AddScoped<IRequestHandler<GetPagedQuery<SupplierDto>, Result<PagedResult<SupplierDto>>>, VNVTStore.Application.Suppliers.Handlers.SupplierHandlers>();
        services.AddScoped<IRequestHandler<DeleteMultipleCommand<TblSupplier>, Result>, VNVTStore.Application.Suppliers.Handlers.SupplierHandlers>();
        services.AddScoped<IRequestHandler<GetStatsQuery<TblSupplier>, Result<EntityStatsDto>>, VNVTStore.Application.Suppliers.Handlers.SupplierHandlers>();

        // Category Specialized Handlers (Note: GetPaged is handled by GetCategoriesHandler, but we register CRUD here)
        // We override Generic Handler for CRUD, but GetPaged might conflict if we register both?
        // Method 1: Register CategoryHandlers for CRUD + GetByCode + Stats
        // Method 2: Let GetCategoriesHandler keep GetPaged.
        services.AddScoped<IRequestHandler<CreateCommand<CreateCategoryDto, CategoryDto>, Result<CategoryDto>>, VNVTStore.Application.Categories.Handlers.CategoryHandlers>();
        services.AddScoped<IRequestHandler<UpdateCommand<UpdateCategoryDto, CategoryDto>, Result<CategoryDto>>, VNVTStore.Application.Categories.Handlers.CategoryHandlers>();
        services.AddScoped<IRequestHandler<DeleteCommand<TblCategory>, Result>, VNVTStore.Application.Categories.Handlers.CategoryHandlers>();
        services.AddScoped<IRequestHandler<GetByCodeQuery<CategoryDto>, Result<CategoryDto>>, VNVTStore.Application.Categories.Handlers.CategoryHandlers>();
        services.AddScoped<IRequestHandler<DeleteMultipleCommand<TblCategory>, Result>, VNVTStore.Application.Categories.Handlers.CategoryHandlers>();
        services.AddScoped<IRequestHandler<GetStatsQuery<TblCategory>, Result<EntityStatsDto>>, VNVTStore.Application.Categories.Handlers.CategoryHandlers>();

        services.AddSingleton<ILogicHubService, LogicHubService>();
        services.AddScoped<ISingularityService, SingularityService>();

        return services;
    }

    private static IServiceCollection AddGenericHandler<TEntity, TResponse, TCreateDto, TUpdateDto>(this IServiceCollection services)
        where TEntity : class, IEntity
        where TResponse : class, IBaseDto, new()
        where TCreateDto : class, new()
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
        services.AddTransient(typeof(IRequestHandler<ExportAllQuery<TResponse>, Result<byte[]>>), handlerType);
        services.AddTransient(typeof(IRequestHandler<GetTemplateQuery<TCreateDto>, Result<byte[]>>), handlerType);
        services.AddTransient(typeof(IRequestHandler<ImportCommand<TCreateDto, TResponse>, Result<int>>), handlerType);

        return services;
    }



}


