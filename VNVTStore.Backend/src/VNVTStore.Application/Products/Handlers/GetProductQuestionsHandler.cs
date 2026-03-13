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
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Products.Queries;

namespace VNVTStore.Application.Products.Handlers;

public class GetProductQuestionsHandler : IRequestHandler<GetProductQuestionsQuery, Result<List<ReviewDto>>>
{
    private readonly IRepository<TblReview> _repository;
    private readonly IMapper _mapper;

    public GetProductQuestionsHandler(
        IRepository<TblReview> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<ReviewDto>>> Handle(GetProductQuestionsQuery request, CancellationToken cancellationToken)
    {
        // Fetch approved questions (OrderItemCode is null) with their answers (ParentCode is set)
        var items = await _repository.AsQueryable()
            .Where(r => r.ProductCode == request.ProductCode 
                                          && r.OrderItemCode == null 
                                          && r.IsActive 
                                          && (r.IsApproved == true || r.ParentCode != null))
                                   .OrderByDescending(r => r.CreatedAt)
                                   .ToListAsync(cancellationToken);

        return Result.Success(_mapper.Map<List<ReviewDto>>(items));
    }
}
