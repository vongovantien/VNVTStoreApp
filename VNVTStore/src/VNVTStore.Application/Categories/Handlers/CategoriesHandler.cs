using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Categories.Queries;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Application.Categories.Handlers;

public class CategoriesHandler : BaseHandler<TblCategory>,
    IRequestHandler<GetPagedQuery<CategoryDto>, Result<PagedResult<CategoryDto>>>,
    IRequestHandler<CreateCommand<CreateCategoryDto, CategoryDto>, Result<CategoryDto>>,
    IRequestHandler<UpdateCommand<UpdateCategoryDto, CategoryDto>, Result<CategoryDto>>,
    IRequestHandler<DeleteCommand<TblCategory>, Result>,
    IRequestHandler<GetByCodeQuery<CategoryDto>, Result<CategoryDto>>
{
    private readonly IRepository<TblProduct> _productRepository;

    public CategoriesHandler(
        IRepository<TblCategory> repository,
        IRepository<TblProduct> productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(repository, unitOfWork, mapper)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<PagedResult<CategoryDto>>> Handle(GetPagedQuery<CategoryDto> request, CancellationToken cancellationToken)
    {
        var searchName = request.Search;
        
        return await GetPagedAsync<CategoryDto>(
            request.PageIndex,
            request.PageSize,
            cancellationToken,
            predicate: c => string.IsNullOrEmpty(searchName) || c.Name.ToLower().Contains(searchName.ToLower()),
            orderBy: q => q.OrderBy(c => c.Name));
    }

    public async Task<Result<CategoryDto>> Handle(CreateCommand<CreateCategoryDto, CategoryDto> request, CancellationToken cancellationToken)
    {
        return await CreateAsync<CreateCategoryDto, CategoryDto>(
            request.Dto,
            cancellationToken,
            c => {
                if (string.IsNullOrEmpty(c.Code))
                {
                    c.Code = $"CAT{DateTime.Now.Ticks.ToString().Substring(12)}";
                }
                c.IsActive = true;
            });
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCommand<UpdateCategoryDto, CategoryDto> request, CancellationToken cancellationToken)
    {
        return await UpdateAsync<UpdateCategoryDto, CategoryDto>(
            request.Code,
            request.Dto,
            MessageConstants.Category,
            cancellationToken);
    }

    public async Task<Result> Handle(DeleteCommand<TblCategory> request, CancellationToken cancellationToken)
    {
        // Check if category has products
        var productCount = await _productRepository
            .AsQueryable()
            .Where(p => p.CategoryCode == request.Code)
            .CountAsync(cancellationToken);

        if (productCount > 0)
        {
            var category = await Repository.GetByCodeAsync(request.Code, cancellationToken);
            return Result.Failure(Error.Conflict(MessageConstants.Conflict,
                MessageConstants.Get(MessageConstants.CategoryHasProducts, category?.Name ?? request.Code, productCount)));
        }

        return await DeleteAsync(request.Code, MessageConstants.Category, cancellationToken, softDelete: false);
    }

    public async Task<Result<CategoryDto>> Handle(GetByCodeQuery<CategoryDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<CategoryDto>(request.Code, MessageConstants.Category, cancellationToken);
    }
}
