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
using VNVTStore.Application.Products.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Products.Handlers;

public class GetTrendingProductsHandler : BaseHandler<TblProduct>,
    IRequestHandler<GetTrendingProductsQuery, Result<List<ProductDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTrendingProductsHandler(
        IRepository<TblProduct> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IApplicationDbContext context)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _context = context;
    }

    public async Task<Result<List<ProductDto>>> Handle(GetTrendingProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.TblProducts
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.ViewCount)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<ProductDto>>(products);

        // Populate child collections (ProductImages, Details, etc.) automatically using BaseHandler logic
        await PopulateCollectionsAsync(dtos, null, cancellationToken).ConfigureAwait(false);

        return Result.Success(dtos);
    }
}
