using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Constants;
using VNVTStore.Application.Products.Queries;

namespace VNVTStore.Application.Products.Handlers;

public class GetProductByCodeHandler : BaseHandler<TblProduct>,
    IRequestHandler<GetProductByCodeQuery, Result<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductByCodeHandler(
        IRepository<TblProduct> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IApplicationDbContext context)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _context = context;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByCodeQuery request, CancellationToken cancellationToken)
    {
        Result<ProductDto> result;
        
        if (request.IncludeChildren)
        {
            result = await GetByCodeIncludeChildrenAsync<ProductDto>(
                request.Code, 
                MessageConstants.Product, 
                cancellationToken,
                includes: q => q.Include(p => p.CategoryCodeNavigation)
                                .Include(p => p.Brand)
                                .Include(p => p.SupplierCodeNavigation)
                                .Include(p => p.TblProductUnits).ThenInclude(pu => pu.Unit)
                                .Include(p => p.TblProductDetails)
                                .Include(p => p.TblProductVariants));
        }
        else
        {
            result = await GetByCodeAsync<ProductDto>(
                request.Code, 
                MessageConstants.Product, 
                cancellationToken);
        }

        if (result.IsSuccess)
        {
            // Update ViewCount
            var product = await _context.TblProducts.FirstOrDefaultAsync(p => p.Code == request.Code, cancellationToken);
            if (product != null)
            {
                // Increment ViewCount using EF Core (or Dapper if preferred for performance)
                // Using a private setter so we might need a method or reflection if it's strictly private
                // Actually, I just added it as public int ViewCount { get; private set; }
                // Let's add an IncrementViewCount method to TblProduct for better encapsulation
                product.IncrementViewCount();
                await _context.SaveChangesAsync(cancellationToken);
                result.Value.ViewCount = product.ViewCount;
            }

            // Calculate SoldCount24h
            var last24h = DateTime.UtcNow.AddDays(-1);
            var soldCount = await _context.TblOrderItems
                .Where(oi => oi.ProductCode == request.Code && oi.CreatedAt >= last24h)
                .SumAsync(oi => (int?)oi.Quantity, cancellationToken) ?? 0;
            
            result.Value.SoldCount24h = soldCount;
        }

        // ProductImages and other collections are automatically populated by BaseHandler.GetByCodeAsync
        if (result.IsSuccess && request.IncludeChildren)
        {
            var ratingData = await _context.TblReviews
                .Where(r => (r.ProductCode == request.Code || (r.OrderItemCodeNavigation != null && r.OrderItemCodeNavigation.ProductCode == request.Code)) && r.IsApproved == true)
                .GroupBy(r => 1)
                .Select(g => new { 
                    AverageRating = g.Average(r => (decimal?)r.Rating) ?? 0, 
                    ReviewCount = g.Count() 
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (ratingData != null)
            {
                result.Value.AverageRating = ratingData.AverageRating;
                result.Value.ReviewCount = ratingData.ReviewCount;
            }
        }

        return result;
    }
}
