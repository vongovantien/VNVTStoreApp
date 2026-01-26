using MediatR;
using VNVTStore.Application.Categories.Queries;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Categories.Handlers;

/// <summary>
/// Handler for specialized Category queries that are not covered by the generic BaseHandler.
/// </summary>
public class CategorySpecialHandlers : 
    IRequestHandler<GetCategoryStatsQuery, Result<CategoryStatsDto>>
{
    private readonly IRepository<TblCategory> _repository;

    public CategorySpecialHandlers(IRepository<TblCategory> repository)
    {
        _repository = repository;
    }

    public async Task<Result<CategoryStatsDto>> Handle(GetCategoryStatsQuery request, CancellationToken cancellationToken)
    {
        var total = await _repository.CountAsync(x => true, cancellationToken);
        var active = await _repository.CountAsync(x => x.IsActive, cancellationToken);
        var main = await _repository.CountAsync(x => x.ParentCode == null, cancellationToken);

        return Result.Success(new CategoryStatsDto
        {
            Total = total,
            Active = active,
            Main = main
        });
    }
}
