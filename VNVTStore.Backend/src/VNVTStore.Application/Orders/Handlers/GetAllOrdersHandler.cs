using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Orders.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Application.Interfaces;

using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Common.Helpers;

namespace VNVTStore.Application.Orders.Handlers;

public class GetAllOrdersHandler : BaseHandler<TblOrder>, 
    IRequestHandler<GetAllOrdersQuery, Result<PagedResult<OrderDto>>>
{
    public GetAllOrdersHandler(
        IRepository<TblOrder> orderRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext) : base(orderRepository, unitOfWork, mapper, dapperContext)
    {
    }

    public async Task<Result<PagedResult<OrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var searchFields = new List<SearchDTO>();

        // Global Search
        if (!string.IsNullOrEmpty(request.search))
        {
            searchFields.Add(new SearchDTO 
            { 
                SearchField = "Code", 
                SearchValue = request.search, 
                SearchCondition = SearchCondition.Contains,
                GroupID = 1,
                CombineCondition = "OR"
            });

            searchFields.Add(new SearchDTO 
            { 
                SearchField = "UserCode", 
                SearchValue = request.search, 
                SearchCondition = SearchCondition.Contains,
                GroupID = 1,
                CombineCondition = "OR"
            });
        }

        // Specific Filters
        if (request.filters != null)
        {
            foreach (var filter in request.filters)
            {
                 if (string.IsNullOrEmpty(filter.Value)) continue;
                 
                 // Handle 'code' filter explicitly if passed in filters dictionary
                 if (filter.Key.Equals("code", StringComparison.OrdinalIgnoreCase)) 
                 {
                     searchFields.Add(new SearchDTO { SearchField = "Code", SearchValue = filter.Value, SearchCondition = SearchCondition.Contains });
                 }
                 // Add other specific field filters here if needed
            }
        }
        
        if (request.status.HasValue)
        {
             searchFields.Add(new SearchDTO { SearchField = "Status", SearchValue = ((int)request.status.Value).ToString(), SearchCondition = SearchCondition.Equal });
        }

        return await GetPagedDapperAsync<OrderDto>(
            request.pageIndex,
            request.pageSize,
            searchFields,
            null, 
            null, 
            null, 
            cancellationToken
        );
    }
}
