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

        // Add filter for active products for public APIs
        if (!searchFields.Any(s => s.SearchField.Equals("IsActive", StringComparison.OrdinalIgnoreCase)))
        {
            searchFields.Add(new SearchDTO { SearchField = "IsActive", SearchValue = true, SearchCondition = SearchCondition.Equal });
        }

        var sortDTO = request.SortDTO ?? new SortDTO { SortBy = request.SortField ?? "CreatedAt", SortDescending = request.SortDescending };

        // Ratings are now in TblProduct, so BaseHandler's GetPagedDapperAsync handles filtering/sorting/mapping automatically.
        var result = await GetPagedDapperAsync<ProductDto>(request.PageIndex, request.PageSize, searchFields, sortDTO, null, request.Fields, cancellationToken);

        if (result.IsSuccess && result.Value.Items.Any())
        {
            using var connection = _dapperContext.CreateConnection();
            if (connection.State != System.Data.ConnectionState.Open) connection.Open();
            
            var productCodes = result.Value.Items.Select(p => p.Code).ToList();
            var baseUrl = _baseUrlService.GetBaseUrl().TrimEnd('/');

            // Fetch Images efficiently
            var imageSql = @"
                SELECT ""MasterCode"", ""Path"", ""OriginalName"", ""Code""
                FROM ""TblFile""
                WHERE ""MasterCode"" = ANY(@Codes) AND ""MasterType"" = 'Product' AND ""IsActive"" = true";
                
            var files = (await SqlMapper.QueryAsync<dynamic>(connection, imageSql, new { Codes = productCodes.ToArray() })).ToList();
            var imageMap = files.GroupBy(f => (string)f.MasterCode).ToDictionary(
                g => g.Key, 
                g => g.Select((f, index) => {
                    var path = (string)(f.Path ?? "");
                    var imgUrl = path.StartsWith("http") ? path : $"{baseUrl}/{path.TrimStart('/')}";
                    return new ProductImageDto
                    {
                        Code = (string)f.Code,
                        ImageURL = imgUrl,
                        AltText = (string)f.OriginalName,
                        IsPrimary = index == 0
                    };
                }).ToList()
            );

            // Populate DTOs
            foreach (var product in result.Value.Items)
            {
                if (imageMap.TryGetValue(product.Code, out var productImages))
                {
                    product.ProductImages = productImages;
                }
                else
                {
                    product.ProductImages = new List<ProductImageDto>();
                }
            }
        }

        return result;
    }
}
