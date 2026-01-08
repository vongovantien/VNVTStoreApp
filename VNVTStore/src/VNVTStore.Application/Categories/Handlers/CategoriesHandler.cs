using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VNVTStore.Application.Categories.Queries;
using VNVTStore.Application.Categories.Commands;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Entities; 


namespace VNVTStore.Application.Categories.Handlers;

public class CategoriesHandler : IRequestHandler<GetCategoriesQuery, Result<PagedResult<CategoryDto>>>
{
    private readonly IRepository<TblCategory> _repository;
    private readonly IMapper _mapper;

    public CategoriesHandler(IRepository<TblCategory> repository, IMapper _mapper)
    {
        _repository = repository;
        this._mapper = _mapper;
    }

    public async Task<Result<PagedResult<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.AsQueryable();

        // Filtering
        if (request.Request.Searching != null && request.Request.Searching.Any())
        {
            foreach (var search in request.Request.Searching)
            {
                if (!string.IsNullOrEmpty(search.Field) && search.Field.ToLower() == "name" && !string.IsNullOrEmpty(search.Value))
                {
                    query = query.Where(c => c.Name.ToLower().Contains(search.Value.ToLower()));
                }
            }
        }

        // Sorting
        if (request.Request.SortDTO != null && !string.IsNullOrEmpty(request.Request.SortDTO.SortBy))
        {
            // Simple sort implementation
            if (request.Request.SortDTO.SortBy.ToLower() == "name")
            {
                query = request.Request.SortDTO.SortDescending 
                    ? query.OrderByDescending(c => c.Name) 
                    : query.OrderBy(c => c.Name);
            }
        }
        else
        {
            query = query.OrderBy(c => c.Name);
        }

        var pageIndex = request.Request.PageIndex ?? 1;
        var pageSize = request.Request.PageSize ?? 10;
        
        var totalItems = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IEnumerable<CategoryDto>>(items);

        return Result.Success(new PagedResult<CategoryDto>(dtos, totalItems, pageIndex, pageSize));
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly IRepository<TblCategory> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCategoryCommandHandler(IRepository<TblCategory> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // Validation?
        if (string.IsNullOrEmpty(dto.Code))
        {
             // Generate code if empty? Or simple validation
             // For now assume user might send it or we generate it. 
             // Let's generate a simple one if missing or check uniqueness
        }
        
        // Manual Simple Code Generation for now if missing
        if (string.IsNullOrEmpty(dto.Code))
        {
             // Use timestamp but ensure it fits in typical column (e.g. 20 chars)
             // Ticks is long, substring(12) gives ~7 digits. CAT + 7 = 10 chars. Safe.
             dto.Code = $"CAT{DateTime.Now.Ticks.ToString().Substring(12)}"; 
        }

        var entity = _mapper.Map<TblCategory>(dto);
        // Ensure mapping works
        if (entity.Code == null) entity.Code = dto.Code;
        entity.IsActive = true;

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<CategoryDto>(entity));
    }
}
