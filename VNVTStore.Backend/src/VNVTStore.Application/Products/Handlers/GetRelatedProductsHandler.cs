using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Dapper;
using System.Data;
using VNVTStore.Application.Products.Queries;

namespace VNVTStore.Application.Products.Handlers;

public class GetRelatedProductsHandler : BaseHandler<TblProduct>,
    IRequestHandler<GetRelatedProductsQuery, Result<List<ProductDto>>>
{
    public GetRelatedProductsHandler(
        IRepository<TblProduct> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
    }

    public async Task<Result<List<ProductDto>>> Handle(GetRelatedProductsQuery request, CancellationToken cancellationToken)
    {
        // 1. Fetch the source product to get Category and Brand
        var sourceProduct = await _repository.FindAsync(p => p.Code == request.ProductCode, cancellationToken);
        if (sourceProduct == null)
        {
            return Result<List<ProductDto>>.Failure(Error.NotFound("Product not found"));
        }

        // 2. Build search criteria for related products
        var searchFields = new List<SearchDTO>
        {
            new SearchDTO { SearchField = "IsActive", SearchValue = true, SearchCondition = SearchCondition.Equal },
            new SearchDTO { SearchField = "Code", SearchValue = request.ProductCode, SearchCondition = SearchCondition.NotEqual }
        };

        // Recommend by same category OR same brand
        // We'll use a custom query or complex filter if BaseHandler supports it.
        // For simplicity, we'll fetch items in the same category first, then fill with same brand if needed.
        
        var categorySearch = new List<SearchDTO>(searchFields)
        {
            new SearchDTO { SearchField = "CategoryCode", SearchValue = sourceProduct.CategoryCode, SearchCondition = SearchCondition.Equal }
        };

        var result = await GetPagedDapperAsync<ProductDto>(1, request.Limit, categorySearch, new SortDTO { SortBy = "CreatedAt", SortDescending = true }, null, null, cancellationToken);
        
        var items = result.Value.Items.ToList();

        // 3. If we don't have enough items, try same brand
        if (items.Count < request.Limit && !string.IsNullOrEmpty(sourceProduct.BrandCode))
        {
            var brandSearch = new List<SearchDTO>(searchFields)
            {
                new SearchDTO { SearchField = "BrandCode", SearchValue = sourceProduct.BrandCode, SearchCondition = SearchCondition.Equal }
            };
            
            // Exclude already fetched products
            if (items.Any())
            {
                brandSearch.Add(new SearchDTO { SearchField = "Code", SearchValue = items.Select(i => i.Code).ToList(), SearchCondition = SearchCondition.NotIn });
            }

            var brandResult = await GetPagedDapperAsync<ProductDto>(1, request.Limit - items.Count, brandSearch, new SortDTO { SortBy = "CreatedAt", SortDescending = true }, null, null, cancellationToken);
            items.AddRange(brandResult.Value.Items);
        }

        // Step 4: Images are already populated by GetPagedDapperAsync via BaseHandler's automatic child collection population

        return Result<List<ProductDto>>.Success(items);
    }
}
