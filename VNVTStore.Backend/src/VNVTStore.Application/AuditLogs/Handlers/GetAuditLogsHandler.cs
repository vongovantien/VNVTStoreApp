using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.AuditLogs.Handlers;

public class GetAuditLogsHandler : BaseHandler<TblAuditLog>,
    IRequestHandler<GetPagedQuery<AuditLogDto>, Result<PagedResult<AuditLogDto>>>
{
    public GetAuditLogsHandler(
        IRepository<TblAuditLog> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
    }

    public async Task<Result<PagedResult<AuditLogDto>>> Handle(GetPagedQuery<AuditLogDto> request, CancellationToken cancellationToken)
    {
        var searchFields = request.Searching ?? new List<SearchDTO>();
        var sortDTO = request.SortDTO ?? new SortDTO { SortBy = request.SortField ?? "CreatedAt", SortDescending = request.SortDescending };
        
        // Simple passthrough to base generic handler using Dapper
        return await GetPagedDapperAsync<AuditLogDto>(
            request.PageIndex, 
            request.PageSize, 
            searchFields, 
            sortDTO, 
            null, 
            request.Fields, 
            cancellationToken);
    }
}
