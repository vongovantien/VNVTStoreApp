using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace VNVTStore.Application.SystemConfig.Queries
{
    public class GetSystemConfigsQuery : IRequest<Result<IEnumerable<SystemConfigDto>>>
    {
    }

    public class GetSystemConfigsQueryHandler : IRequestHandler<GetSystemConfigsQuery, Result<IEnumerable<SystemConfigDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetSystemConfigsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<IEnumerable<SystemConfigDto>>> Handle(GetSystemConfigsQuery request, CancellationToken cancellationToken)
        {
            var configs = await _context.TblSystemConfigs
                .AsNoTracking()
                .OrderBy(c => c.Code)
                .Select(c => new SystemConfigDto
                {
                    ConfigKey = c.Code,
                    ConfigValue = c.ConfigValue,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return Result<IEnumerable<SystemConfigDto>>.Success(configs);
        }
    }
}
