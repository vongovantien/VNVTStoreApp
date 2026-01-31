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
using VNVTStore.Application.Common.Helpers;

namespace VNVTStore.Application.Products.Handlers;

public class GetProductsHandler : BaseHandler<TblProduct>,
    IRequestHandler<GetPagedQuery<ProductDto>, Result<PagedResult<ProductDto>>>
{
    private readonly IBaseUrlService _baseUrlService;

    public GetProductsHandler(
        IRepository<TblProduct> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IBaseUrlService baseUrlService)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _baseUrlService = baseUrlService;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetPagedQuery<ProductDto> request, CancellationToken cancellationToken)
    {
        var searchFields = request.Searching ?? new List<SearchDTO>();

        if (!string.IsNullOrEmpty(request.Search))
        {
            searchFields.Add(new SearchDTO { SearchField = "Name", SearchValue = request.Search, SearchCondition = SearchCondition.Contains, GroupID = 1, CombineCondition = "OR" });
            searchFields.Add(new SearchDTO { SearchField = "Code", SearchValue = request.Search, SearchCondition = SearchCondition.Contains, GroupID = 1, CombineCondition = "OR" });
            searchFields.Add(new SearchDTO { SearchField = "Description", SearchValue = request.Search, SearchCondition = SearchCondition.Contains, GroupID = 1, CombineCondition = "OR" });
        }

        var sortDTO = request.SortDTO ?? new SortDTO { SortBy = request.SortField ?? "CreatedAt", SortDescending = request.SortDescending };

        var result = await GetPagedDapperAsync<ProductDto>(request.PageIndex, request.PageSize, searchFields, sortDTO, null, request.Fields, cancellationToken);

        if (result.IsSuccess && result.Value.Items.Any())
        {
            using var connection = _dapperContext.CreateConnection();
            var productCodes = result.Value.Items.Select(p => p.Code).ToList();
            var fileSql = @"SELECT * FROM ""TblFile"" WHERE ""MasterCode"" = ANY(@Codes) AND ""MasterType"" = 'Product'";
            var files = await connection.QueryAsync<TblFile>(fileSql, new { Codes = productCodes });
            
            var fileGroups = files
                .GroupBy(f => f.MasterCode)
                .Where(g => g.Key != null)
                .ToDictionary(g => g.Key!, g => g.ToList());
            var baseUrl = _baseUrlService.GetBaseUrl().TrimEnd('/');

            foreach (var dto in result.Value.Items)
            {
                if (fileGroups.TryGetValue(dto.Code, out var productFiles))
                {
                    dto.ProductImages = productFiles.Select(f => new ProductImageDto
                    {
                        Code = f.Code,
                        ImageURL = f.Path.StartsWith("http") ? f.Path : $"{baseUrl}/{f.Path.TrimStart('/')}",
                        AltText = f.OriginalName,
                        IsPrimary = productFiles.IndexOf(f) == 0 
                    }).ToList();
                }
            }
        }

        return result;
    }
}
