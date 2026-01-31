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
    private readonly IBaseUrlService _baseUrlService;
    private readonly IApplicationDbContext _context;

    public GetProductByCodeHandler(
        IRepository<TblProduct> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IBaseUrlService baseUrlService,
        IApplicationDbContext context)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _baseUrlService = baseUrlService;
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

        if (result.IsSuccess && request.IncludeChildren)
        {
            var files = await _context.TblFiles
                .Where(f => f.MasterCode == request.Code && (f.MasterType == "Product" || f.MasterType == "PRODUCT"))
                .ToListAsync(cancellationToken);
            
            var baseUrl = _baseUrlService.GetBaseUrl().TrimEnd('/');
            result.Value.ProductImages = files.Select(f => new ProductImageDto
            {
                 Code = f.Code,
                 ImageURL = f.Path.StartsWith("http") ? f.Path : $"{baseUrl}/{f.Path.TrimStart('/')}",
                 AltText = f.OriginalName,
                 IsPrimary = files.IndexOf(f) == 0
            }).ToList();
        }

        return result;
    }
}
