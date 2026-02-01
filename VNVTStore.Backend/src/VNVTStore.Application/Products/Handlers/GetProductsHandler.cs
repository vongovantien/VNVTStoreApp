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

        // Add filter for active products for public APIs
        if (!searchFields.Any(s => s.SearchField.Equals("IsActive", StringComparison.OrdinalIgnoreCase)))
        {
            searchFields.Add(new SearchDTO { SearchField = "IsActive", SearchValue = true, SearchCondition = SearchCondition.Equal });
        }

        var sortDTO = request.SortDTO ?? new SortDTO { SortBy = request.SortField ?? "CreatedAt", SortDescending = request.SortDescending };

        var result = await GetPagedDapperAsync<ProductDto>(request.PageIndex, request.PageSize, searchFields, sortDTO, null, request.Fields, cancellationToken);

        if (result.IsSuccess && result.Value.Items.Any())
        {
            using var connection = _dapperContext.CreateConnection();
            if (connection.State != System.Data.ConnectionState.Open) connection.Open();
            
            var productCodes = result.Value.Items.Select(p => p.Code).ToList();
            
            Log.Information("[Debug] GetProductsHandler manual image fetch for {Count} products. Codes: {Codes}", 
                productCodes.Count, string.Join(",", productCodes));

            var fileSql = "SELECT * FROM \"TblFile\" WHERE \"MasterCode\" = ANY(@Codes) AND \"MasterType\" ILIKE 'Product'";
            var files = (await connection.QueryAsync<TblFile>(fileSql, new { Codes = productCodes.ToArray() })).ToList();
            
            Log.Information("[Debug] Found {Count} images in TblFile. First few MasterCodes: {Samples}", 
                files.Count, string.Join(",", files.Take(5).Select(f => f.MasterCode)));

            var fileGroups = files
                .GroupBy(f => f.MasterCode)
                .Where(g => g.Key != null)
                .ToDictionary(g => g.Key!, g => g.ToList());
            
            var baseUrl = _baseUrlService.GetBaseUrl().TrimEnd('/');

            // 2. Fetch Ratings and Review Counts
            var ratingSql = @"
                SELECT 
                    COALESCE(""ProductCode"", (SELECT oi.""ProductCode"" FROM ""TblOrderItem"" oi WHERE oi.""Code"" = r.""OrderItemCode"")) as ProductCode, 
                    AVG(CAST(""Rating"" AS DECIMAL)) as AverageRating, 
                    COUNT(*) as ReviewCount
                FROM ""TblReview"" r
                WHERE (""ProductCode"" = ANY(@Codes) OR (SELECT oi.""ProductCode"" FROM ""TblOrderItem"" oi WHERE oi.""Code"" = r.""OrderItemCode"") = ANY(@Codes))
                  AND ""IsApproved"" = true
                GROUP BY 1";
            
            var ratings = (await connection.QueryAsync<dynamic>(ratingSql, new { Codes = productCodes.ToArray() })).ToList();
            var ratingMap = ratings.ToDictionary(
                r => (string)r.ProductCode, 
                r => (AverageRating: (decimal)r.AverageRating, ReviewCount: (int)r.ReviewCount)
            );

            foreach (var dto in result.Value.Items)
            {
                // Populate Images
                if (fileGroups.TryGetValue(dto.Code, out var productFiles))
                {
                    dto.ProductImages = productFiles.Select(f => {
                        var path = f.Path ?? "";
                        var imgUrl = path.StartsWith("http") ? path : $"{baseUrl}/{path.TrimStart('/')}";
                        return new ProductImageDto
                        {
                            Code = f.Code,
                            ImageURL = imgUrl,
                            AltText = f.OriginalName,
                            IsPrimary = productFiles.IndexOf(f) == 0 
                        };
                    }).ToList();
                }
                else 
                {
                    dto.ProductImages = new List<ProductImageDto>();
                }

                // Populate Ratings
                if (ratingMap.TryGetValue(dto.Code, out var ratingInfo))
                {
                    dto.AverageRating = ratingInfo.AverageRating;
                    dto.ReviewCount = ratingInfo.ReviewCount;
                }
                else 
                {
                    dto.AverageRating = 0;
                    dto.ReviewCount = 0;
                }
            }
        }

        return result;
    }
}
