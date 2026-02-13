using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using AutoMapper;

namespace VNVTStore.Application.Categories.Handlers;

public class GetCategoriesHandler : BaseHandler<TblCategory>,
    IRequestHandler<GetPagedQuery<CategoryDto>, Result<PagedResult<CategoryDto>>>
{
    private readonly IBaseUrlService _baseUrlService;

    public GetCategoriesHandler(
        IRepository<TblCategory> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IBaseUrlService baseUrlService)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _baseUrlService = baseUrlService;
    }

    public async Task<Result<PagedResult<CategoryDto>>> Handle(GetPagedQuery<CategoryDto> request, CancellationToken cancellationToken)
    {
        // 1. Call standard BaseHandler which now populates Files via ReferenceCollection
        var result = await GetPagedDapperAsync<CategoryDto>(request.PageIndex, request.PageSize, request.Searching, request.SortDTO, null, request.Fields, cancellationToken);

        if (result.IsSuccess && result.Value.Items.Any())
        {
            var baseUrl = _baseUrlService.GetBaseUrl().TrimEnd('/');
            
            foreach (var category in result.Value.Items)
            {
                // Map first file to ImageUrl
                // TblFile.Path is in category.Files[0].Path
                var file = category.Files?.FirstOrDefault();
                if (file != null && !string.IsNullOrEmpty(file.Path))
                {
                    var path = file.Path;
                    category.ImageUrl = path.StartsWith("http") ? path : $"{baseUrl}/{path.TrimStart('/')}";
                }
            }
        }

        return result;
    }
}
