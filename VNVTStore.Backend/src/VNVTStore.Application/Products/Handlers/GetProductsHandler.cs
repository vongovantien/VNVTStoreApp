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
using Serilog;
using System.Data;
using VNVTStore.Application.Common.Helpers;

namespace VNVTStore.Application.Products.Handlers;

public class GetProductsHandler : BaseHandler<TblProduct>,
    IRequestHandler<GetPagedQuery<ProductDto>, Result<PagedResult<ProductDto>>>
{
    public GetProductsHandler(
        IRepository<TblProduct> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetPagedQuery<ProductDto> request, CancellationToken cancellationToken)
    {
        var searchFields = request.Searching ?? new List<SearchDTO>();

        // Add filter for active products for public APIs
        if (!searchFields.Any(s => s.SearchField.Equals("IsActive", StringComparison.OrdinalIgnoreCase)))
        {
            searchFields.Add(new SearchDTO { SearchField = "IsActive", SearchValue = true, SearchCondition = SearchCondition.Equal });
        }

        var sortDTO = request.SortDTO ?? new SortDTO { SortBy = request.SortField ?? "CreatedAt", SortDescending = request.SortDescending };

        // Ratings are now in TblProduct, so BaseHandler's GetPagedDapperAsync handles filtering/sorting/mapping automatically.
        var result = await GetPagedDapperAsync<ProductDto>(request.PageIndex, request.PageSize, searchFields, sortDTO, null, request.Fields, cancellationToken);

        return result;
    }
}
